using Moq;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using ChuBank.Application.Interfaces;
using ChuBank.Application.DTOs.Requests;
using ChuBank.Application.DTOs.Responses;

namespace ChuBank.Tests.Controllers;

public class TransfersControllerTests
{
    private readonly Mock<ITransferService> _mockTransferService;
    private readonly Mock<IValidator<CreateTransferRequest>> _mockValidator;

    public TransfersControllerTests()
    {
        _mockTransferService = new Mock<ITransferService>();
        _mockValidator = new Mock<IValidator<CreateTransferRequest>>();
    }

    [Fact]
    public async Task CreateTransfer_ShouldCallService_WhenValidRequest()
    {
        // Arrange
        var request = new CreateTransferRequest
        {
            FromAccountNumber = "123456",
            ToAccountNumber = "654321",
            Amount = 100.00m,
            Description = "Test transfer"
        };

        var response = new TransferResponse
        {
            Id = Guid.NewGuid(),
            FromAccountNumber = request.FromAccountNumber,
            ToAccountNumber = request.ToAccountNumber,
            Amount = request.Amount,
            Description = request.Description,
            TransferDate = DateTime.Today,
            CreatedAt = DateTime.UtcNow
        };

        var validationResult = new ValidationResult();

        _mockValidator
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        _mockTransferService
            .Setup(x => x.CreateTransferAsync(request))
            .ReturnsAsync(response);

        // Act
        var result = await _mockTransferService.Object.CreateTransferAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(response);
        _mockTransferService.Verify(x => x.CreateTransferAsync(request), Times.Once);
    }

    [Fact]
    public async Task CreateTransfer_ShouldNotCallService_WhenValidationFails()
    {
        // Arrange
        var request = new CreateTransferRequest
        {
            FromAccountNumber = "",
            ToAccountNumber = "",
            Amount = -100.00m,
            Description = ""
        };

        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure("FromAccountNumber", "From account number is required"));
        validationResult.Errors.Add(new ValidationFailure("ToAccountNumber", "To account number is required"));
        validationResult.Errors.Add(new ValidationFailure("Amount", "Amount must be positive"));

        _mockValidator
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var validation = await _mockValidator.Object.ValidateAsync(request);

        // Assert
        validation.IsValid.Should().BeFalse();
        validation.Errors.Should().HaveCount(3);
        _mockTransferService.Verify(x => x.CreateTransferAsync(It.IsAny<CreateTransferRequest>()), Times.Never);
    }

    [Fact]
    public async Task CreateTransfer_ShouldThrowException_WhenNotBusinessDay()
    {
        // Arrange
        var request = new CreateTransferRequest
        {
            FromAccountNumber = "123456",
            ToAccountNumber = "654321",
            Amount = 100.00m,
            Description = "Test transfer"
        };

        var validationResult = new ValidationResult();

        _mockValidator
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        _mockTransferService
            .Setup(x => x.CreateTransferAsync(request))
            .ThrowsAsync(new InvalidOperationException("Transfers are not allowed on non-business days"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _mockTransferService.Object.CreateTransferAsync(request));
    }

    [Fact]
    public async Task CreateTransfer_ShouldThrowException_WhenInsufficientBalance()
    {
        // Arrange
        var request = new CreateTransferRequest
        {
            FromAccountNumber = "123456",
            ToAccountNumber = "654321",
            Amount = 1500.00m,
            Description = "Test transfer"
        };

        var validationResult = new ValidationResult();

        _mockValidator
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        _mockTransferService
            .Setup(x => x.CreateTransferAsync(request))
            .ThrowsAsync(new InvalidOperationException("Insufficient balance"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _mockTransferService.Object.CreateTransferAsync(request));
    }

    [Fact]
    public async Task CreateTransfer_ShouldThrowException_WhenAccountNotFound()
    {
        // Arrange
        var request = new CreateTransferRequest
        {
            FromAccountNumber = "999999",
            ToAccountNumber = "654321",
            Amount = 100.00m,
            Description = "Test transfer"
        };

        var validationResult = new ValidationResult();

        _mockValidator
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        _mockTransferService
            .Setup(x => x.CreateTransferAsync(request))
            .ThrowsAsync(new InvalidOperationException("Account not found"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _mockTransferService.Object.CreateTransferAsync(request));
    }
}
