using Moq;
using FluentAssertions;
using ChuBank.Application.Services;
using ChuBank.Application.DTOs.Requests;
using ChuBank.Domain.Entities;
using ChuBank.Domain.Interfaces;

namespace ChuBank.Tests.Services;

public class AccountServiceTests
{
    private readonly Mock<IAccountRepository> _mockAccountRepository;
    private readonly Mock<ILogService> _mockLogService;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly AccountService _accountService;

    public AccountServiceTests()
    {
        _mockAccountRepository = new Mock<IAccountRepository>();
        _mockLogService = new Mock<ILogService>();
        _mockCacheService = new Mock<ICacheService>();
        _accountService = new AccountService(_mockAccountRepository.Object, _mockLogService.Object, _mockCacheService.Object);
    }

    [Fact]
    public async Task CreateAccountAsync_ShouldCreateAccount_WhenValidRequest()
    {
        // Arrange
        var request = new CreateAccountRequest
        {
            HolderName = "João Silva",
            InitialBalance = 1000.00m
        };

        var expectedAccount = new Account
        {
            Id = Guid.NewGuid(),
            AccountNumber = "123456",
            HolderName = request.HolderName,
            Balance = request.InitialBalance,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _mockAccountRepository
            .Setup(x => x.GetByAccountNumberAsync(It.IsAny<string>()))
            .ReturnsAsync((Account?)null);

        _mockAccountRepository
            .Setup(x => x.CreateAsync(It.IsAny<Account>()))
            .ReturnsAsync(expectedAccount);

        // Act
        var result = await _accountService.CreateAccountAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.HolderName.Should().Be(request.HolderName);
        result.Balance.Should().Be(request.InitialBalance);
        result.IsActive.Should().BeTrue();
        _mockAccountRepository.Verify(x => x.CreateAsync(It.IsAny<Account>()), Times.Once);
    }

    [Fact]
    public async Task GetAccountByIdAsync_ShouldReturnAccount_WhenAccountExists()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = new Account
        {
            Id = accountId,
            AccountNumber = "123456",
            HolderName = "João Silva",
            Balance = 1000.00m,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _mockAccountRepository
            .Setup(x => x.GetByIdAsync(accountId))
            .ReturnsAsync(account);

        // Act
        var result = await _accountService.GetAccountByIdAsync(accountId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(accountId);
        result.HolderName.Should().Be(account.HolderName);
        result.Balance.Should().Be(account.Balance);
    }

    [Fact]
    public async Task GetAccountByIdAsync_ShouldReturnNull_WhenAccountDoesNotExist()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        _mockAccountRepository
            .Setup(x => x.GetByIdAsync(accountId))
            .ReturnsAsync((Account?)null);

        // Act
        var result = await _accountService.GetAccountByIdAsync(accountId);

        // Assert
        result.Should().BeNull();
    }
}