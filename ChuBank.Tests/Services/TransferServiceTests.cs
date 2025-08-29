using Moq;
using FluentAssertions;
using ChuBank.Application.Services;
using ChuBank.Application.DTOs.Requests;
using ChuBank.Domain.Entities;
using ChuBank.Domain.Interfaces;

namespace ChuBank.Tests.Services;

public class TransferServiceTests
{
    private readonly Mock<ITransferRepository> _mockTransferRepository;
    private readonly Mock<IAccountRepository> _mockAccountRepository;
    private readonly Mock<IHolidayService> _mockHolidayService;
    private readonly Mock<ILogService> _mockLogService;
    private readonly TransferService _transferService;

    public TransferServiceTests()
    {
        _mockTransferRepository = new Mock<ITransferRepository>();
        _mockAccountRepository = new Mock<IAccountRepository>();
        _mockHolidayService = new Mock<IHolidayService>();
        _mockLogService = new Mock<ILogService>();
        _transferService = new TransferService(
            _mockTransferRepository.Object,
            _mockAccountRepository.Object,
            _mockHolidayService.Object,
            _mockLogService.Object);
    }

    [Fact]
    public async Task CreateTransferAsync_ShouldCreateTransfer_WhenValidRequest()
    {
        var request = new CreateTransferRequest
        {
            FromAccountNumber = "123456",
            ToAccountNumber = "654321",
            Amount = 100.00m,
            Description = "Test transfer"
        };

        var fromAccount = new Account
        {
            Id = Guid.NewGuid(),
            AccountNumber = "123456",
            HolderName = "João Silva",
            Balance = 1000.00m,
            IsActive = true
        };

        var toAccount = new Account
        {
            Id = Guid.NewGuid(),
            AccountNumber = "654321",
            HolderName = "Maria Santos",
            Balance = 500.00m,
            IsActive = true
        };

        var expectedTransfer = new Transfer
        {
            Id = Guid.NewGuid(),
            FromAccountId = fromAccount.Id,
            ToAccountId = toAccount.Id,
            Amount = request.Amount,
            Description = request.Description,
            TransferDate = DateTime.Today,
            CreatedAt = DateTime.UtcNow
        };

        _mockHolidayService
            .Setup(x => x.IsBusinessDayAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(true);

        _mockAccountRepository
            .Setup(x => x.GetByAccountNumberAsync("123456"))
            .ReturnsAsync(fromAccount);

        _mockAccountRepository
            .Setup(x => x.GetByAccountNumberAsync("654321"))
            .ReturnsAsync(toAccount);

        _mockAccountRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Account>()))
            .ReturnsAsync((Account account) => account);

        _mockTransferRepository
            .Setup(x => x.CreateAsync(It.IsAny<Transfer>()))
            .ReturnsAsync(expectedTransfer);

        var result = await _transferService.CreateTransferAsync(request);

        result.Should().NotBeNull();
        result.Amount.Should().Be(request.Amount);
        result.FromAccountNumber.Should().Be(request.FromAccountNumber);
        result.ToAccountNumber.Should().Be(request.ToAccountNumber);
        result.Description.Should().Be(request.Description);
        
        _mockAccountRepository.Verify(x => x.UpdateAsync(It.IsAny<Account>()), Times.Exactly(2));
        _mockTransferRepository.Verify(x => x.CreateAsync(It.IsAny<Transfer>()), Times.Once);
    }

    [Fact]
    public async Task CreateTransferAsync_ShouldThrowException_WhenNotBusinessDay()
    {
        var request = new CreateTransferRequest
        {
            FromAccountNumber = "123456",
            ToAccountNumber = "654321",
            Amount = 100.00m
        };

        _mockHolidayService
            .Setup(x => x.IsBusinessDayAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _transferService.CreateTransferAsync(request));
    }

    [Fact]
    public async Task CreateTransferAsync_ShouldThrowException_WhenInsufficientBalance()
    {
        var request = new CreateTransferRequest
        {
            FromAccountNumber = "123456",
            ToAccountNumber = "654321",
            Amount = 1500.00m
        };

        var fromAccount = new Account
        {
            Id = Guid.NewGuid(),
            AccountNumber = "123456",
            Balance = 1000.00m,
            IsActive = true
        };

        var toAccount = new Account
        {
            Id = Guid.NewGuid(),
            AccountNumber = "654321",
            Balance = 500.00m,
            IsActive = true
        };

        _mockHolidayService
            .Setup(x => x.IsBusinessDayAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(true);

        _mockAccountRepository
            .Setup(x => x.GetByAccountNumberAsync("123456"))
            .ReturnsAsync(fromAccount);

        _mockAccountRepository
            .Setup(x => x.GetByAccountNumberAsync("654321"))
            .ReturnsAsync(toAccount);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _transferService.CreateTransferAsync(request));
    }
}
