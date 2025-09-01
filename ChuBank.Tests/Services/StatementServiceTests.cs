using Moq;
using FluentAssertions;
using ChuBank.Application.Services;
using ChuBank.Application.DTOs.Responses;
using ChuBank.Domain.Entities;
using ChuBank.Domain.Interfaces;

namespace ChuBank.Tests.Services;

public class StatementServiceTests
{
    private readonly Mock<IStatementRepository> _mockStatementRepository;
    private readonly Mock<ITransferRepository> _mockTransferRepository;
    private readonly Mock<IAccountRepository> _mockAccountRepository;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly StatementService _statementService;

    public StatementServiceTests()
    {
        _mockStatementRepository = new Mock<IStatementRepository>();
        _mockTransferRepository = new Mock<ITransferRepository>();
        _mockAccountRepository = new Mock<IAccountRepository>();
        _mockCacheService = new Mock<ICacheService>();
        _statementService = new StatementService(
            _mockStatementRepository.Object,
            _mockTransferRepository.Object,
            _mockAccountRepository.Object,
            _mockCacheService.Object);
    }

    [Fact]
    public async Task GenerateStatementAsync_ShouldReturnCachedStatement_WhenCacheExists()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);
        var cacheKey = $"statement_{accountId}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";

        var cachedStatement = new StatementResponse
        {
            Id = Guid.NewGuid(),
            AccountNumber = "123456",
            StartDate = startDate,
            EndDate = endDate,
            OpeningBalance = 1000.00m,
            ClosingBalance = 800.00m,
            GeneratedAt = DateTime.UtcNow,
            Entries = new List<StatementEntryResponse>()
        };

        _mockCacheService
            .Setup(x => x.GetAsync<StatementResponse>(cacheKey))
            .ReturnsAsync(cachedStatement);

        // Act
        var result = await _statementService.GenerateStatementAsync(accountId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(cachedStatement);
        _mockAccountRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _mockTransferRepository.Verify(x => x.GetByAccountIdAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task GenerateStatementAsync_ShouldThrowException_WhenAccountNotFound()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);

        _mockCacheService
            .Setup(x => x.GetAsync<StatementResponse>(It.IsAny<string>()))
            .ReturnsAsync((StatementResponse?)null);

        _mockAccountRepository
            .Setup(x => x.GetByIdAsync(accountId))
            .ReturnsAsync((Account?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _statementService.GenerateStatementAsync(accountId, startDate, endDate));
    }

    [Fact]
    public async Task GenerateStatementAsync_ShouldGenerateStatement_WhenValidRequest()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);

        var account = new Account
        {
            Id = accountId,
            AccountNumber = "123456",
            HolderName = "João Silva",
            Balance = 800.00m,
            IsActive = true
        };

        var fromAccount = new Account
        {
            Id = Guid.NewGuid(),
            AccountNumber = "654321",
            HolderName = "Maria Santos"
        };

        var transfers = new List<Transfer>
        {
            new Transfer
            {
                Id = Guid.NewGuid(),
                FromAccountId = fromAccount.Id,
                ToAccountId = accountId,
                Amount = 200.00m,
                Description = "Test credit",
                TransferDate = new DateTime(2025, 1, 15),
                FromAccount = fromAccount,
                ToAccount = account
            },
            new Transfer
            {
                Id = Guid.NewGuid(),
                FromAccountId = accountId,
                ToAccountId = fromAccount.Id,
                Amount = 100.00m,
                Description = "Test debit",
                TransferDate = new DateTime(2025, 1, 20),
                FromAccount = account,
                ToAccount = fromAccount
            }
        };

        _mockCacheService
            .Setup(x => x.GetAsync<StatementResponse>(It.IsAny<string>()))
            .ReturnsAsync((StatementResponse?)null);

        _mockAccountRepository
            .Setup(x => x.GetByIdAsync(accountId))
            .ReturnsAsync(account);

        _mockTransferRepository
            .Setup(x => x.GetByAccountIdAsync(accountId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(transfers);

        _mockTransferRepository
            .Setup(x => x.GetByAccountIdAsync(accountId, null, It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Transfer>());

        // Act
        var result = await _statementService.GenerateStatementAsync(accountId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.AccountNumber.Should().Be(account.AccountNumber);
        result.StartDate.Should().Be(startDate.Date);
        result.EndDate.Should().Be(endDate.Date);
        result.OpeningBalance.Should().Be(0);
        result.ClosingBalance.Should().Be(100.00m);
        result.Entries.Should().HaveCount(2);
        
        var creditEntry = result.Entries.First();
        creditEntry.Type.Should().Be("CREDIT");
        creditEntry.Amount.Should().Be(200.00m);
        creditEntry.Balance.Should().Be(200.00m);

        var debitEntry = result.Entries.Last();
        debitEntry.Type.Should().Be("DEBIT");
        debitEntry.Amount.Should().Be(-100.00m);
        debitEntry.Balance.Should().Be(100.00m);

        _mockCacheService.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<StatementResponse>(), It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task GenerateStatementAsync_ShouldCalculateOpeningBalance_WhenPreviousTransfersExist()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var startDate = new DateTime(2025, 1, 15);
        var endDate = new DateTime(2025, 1, 31);

        var account = new Account
        {
            Id = accountId,
            AccountNumber = "123456",
            HolderName = "João Silva",
            Balance = 1200.00m,
            IsActive = true
        };

        var otherAccountId = Guid.NewGuid();
        var otherAccount = new Account
        {
            Id = otherAccountId,
            AccountNumber = "654321",
            HolderName = "Maria Santos"
        };

        var previousTransfers = new List<Transfer>
        {
            new Transfer
            {
                Id = Guid.NewGuid(),
                FromAccountId = otherAccountId,
                ToAccountId = accountId,
                Amount = 500.00m,
                TransferDate = new DateTime(2025, 1, 10),
                FromAccount = otherAccount,
                ToAccount = account
            },
            new Transfer
            {
                Id = Guid.NewGuid(),
                FromAccountId = accountId,
                ToAccountId = otherAccountId,
                Amount = 200.00m,
                TransferDate = new DateTime(2025, 1, 12),
                FromAccount = account,
                ToAccount = otherAccount
            }
        };

        var periodTransfers = new List<Transfer>();

        _mockCacheService
            .Setup(x => x.GetAsync<StatementResponse>(It.IsAny<string>()))
            .ReturnsAsync((StatementResponse?)null);

        _mockAccountRepository
            .Setup(x => x.GetByIdAsync(accountId))
            .ReturnsAsync(account);

        _mockTransferRepository
            .Setup(x => x.GetByAccountIdAsync(accountId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(periodTransfers);

        _mockTransferRepository
            .Setup(x => x.GetByAccountIdAsync(accountId, null, It.IsAny<DateTime>()))
            .ReturnsAsync(previousTransfers);

        // Act
        var result = await _statementService.GenerateStatementAsync(accountId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.OpeningBalance.Should().Be(300.00m); // 500 - 200
        result.ClosingBalance.Should().Be(300.00m); // Sem transferências no período
        result.Entries.Should().BeEmpty();
    }
}
