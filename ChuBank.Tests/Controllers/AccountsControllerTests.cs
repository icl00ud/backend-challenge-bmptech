using Moq;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using ChuBank.Application.Interfaces;
using ChuBank.Application.DTOs.Requests;
using ChuBank.Application.DTOs.Responses;

namespace ChuBank.Tests.Controllers;

public class AccountsControllerTests
{
    private readonly Mock<IAccountService> _mockAccountService;
    private readonly Mock<IValidator<CreateAccountRequest>> _mockValidator;

    public AccountsControllerTests()
    {
        _mockAccountService = new Mock<IAccountService>();
        _mockValidator = new Mock<IValidator<CreateAccountRequest>>();
    }

    [Fact]
    public async Task CreateAccount_ShouldCallService_WhenValidRequest()
    {
        // Arrange
        var request = new CreateAccountRequest
        {
            HolderName = "João Silva",
            InitialBalance = 1000.00m
        };

        var response = new AccountResponse
        {
            Id = Guid.NewGuid(),
            AccountNumber = "123456",
            HolderName = request.HolderName,
            Balance = request.InitialBalance,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var validationResult = new ValidationResult();

        _mockValidator
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        _mockAccountService
            .Setup(x => x.CreateAccountAsync(request))
            .ReturnsAsync(response);

        // Act & Assert
        var result = await _mockAccountService.Object.CreateAccountAsync(request);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(response);
        _mockAccountService.Verify(x => x.CreateAccountAsync(request), Times.Once);
    }

    [Fact]
    public async Task CreateAccount_ShouldNotCallService_WhenValidationFails()
    {
        // Arrange
        var request = new CreateAccountRequest
        {
            HolderName = "",
            InitialBalance = -100.00m
        };

        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure("HolderName", "Holder name is required"));
        validationResult.Errors.Add(new ValidationFailure("InitialBalance", "Initial balance must be positive"));

        _mockValidator
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var validation = await _mockValidator.Object.ValidateAsync(request);

        // Assert
        validation.IsValid.Should().BeFalse();
        validation.Errors.Should().HaveCount(2);
        _mockAccountService.Verify(x => x.CreateAccountAsync(It.IsAny<CreateAccountRequest>()), Times.Never);
    }

    [Fact]
    public async Task GetAccount_ShouldReturnAccount_WhenAccountExists()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var response = new AccountResponse
        {
            Id = accountId,
            AccountNumber = "123456",
            HolderName = "João Silva",
            Balance = 1000.00m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _mockAccountService
            .Setup(x => x.GetAccountByIdAsync(accountId))
            .ReturnsAsync(response);

        // Act
        var result = await _mockAccountService.Object.GetAccountByIdAsync(accountId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(response);
        _mockAccountService.Verify(x => x.GetAccountByIdAsync(accountId), Times.Once);
    }

    [Fact]
    public async Task GetAccount_ShouldReturnNull_WhenAccountDoesNotExist()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        _mockAccountService
            .Setup(x => x.GetAccountByIdAsync(accountId))
            .ReturnsAsync((AccountResponse?)null);

        // Act
        var result = await _mockAccountService.Object.GetAccountByIdAsync(accountId);

        // Assert
        result.Should().BeNull();
        _mockAccountService.Verify(x => x.GetAccountByIdAsync(accountId), Times.Once);
    }

    [Fact]
    public async Task GetAllAccounts_ShouldReturnAccounts_WhenAccountsExist()
    {
        // Arrange
        var accounts = new List<AccountResponse>
        {
            new AccountResponse
            {
                Id = Guid.NewGuid(),
                AccountNumber = "123456",
                HolderName = "João Silva",
                Balance = 1000.00m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new AccountResponse
            {
                Id = Guid.NewGuid(),
                AccountNumber = "654321",
                HolderName = "Maria Santos",
                Balance = 2000.00m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockAccountService
            .Setup(x => x.GetAllAccountsAsync())
            .ReturnsAsync(accounts);

        // Act
        var result = await _mockAccountService.Object.GetAllAccountsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(accounts);
        _mockAccountService.Verify(x => x.GetAllAccountsAsync(), Times.Once);
    }
}
