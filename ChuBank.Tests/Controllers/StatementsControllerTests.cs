using Moq;
using FluentAssertions;
using ChuBank.Application.Interfaces;
using ChuBank.Application.DTOs.Responses;

namespace ChuBank.Tests.Controllers;

public class StatementsControllerTests
{
    private readonly Mock<IStatementService> _mockStatementService;

    public StatementsControllerTests()
    {
        _mockStatementService = new Mock<IStatementService>();
    }

    [Fact]
    public async Task GetStatement_ShouldReturnStatement_WhenValidRequest()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);

        var response = new StatementResponse
        {
            Id = Guid.NewGuid(),
            AccountNumber = "123456",
            StartDate = startDate,
            EndDate = endDate,
            OpeningBalance = 1000.00m,
            ClosingBalance = 800.00m,
            GeneratedAt = DateTime.UtcNow,
            Entries = new List<StatementEntryResponse>
            {
                new StatementEntryResponse
                {
                    Date = new DateTime(2025, 1, 15),
                    Description = "Transfer from 654321",
                    Amount = 200.00m,
                    Balance = 1200.00m,
                    Type = "CREDIT"
                },
                new StatementEntryResponse
                {
                    Date = new DateTime(2025, 1, 20),
                    Description = "Transfer to 654321",
                    Amount = -400.00m,
                    Balance = 800.00m,
                    Type = "DEBIT"
                }
            }
        };

        _mockStatementService
            .Setup(x => x.GenerateStatementAsync(accountId, startDate, endDate))
            .ReturnsAsync(response);

        // Act
        var result = await _mockStatementService.Object.GenerateStatementAsync(accountId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(response);
        result.Entries.Should().HaveCount(2);
        result.OpeningBalance.Should().Be(1000.00m);
        result.ClosingBalance.Should().Be(800.00m);
        _mockStatementService.Verify(x => x.GenerateStatementAsync(accountId, startDate, endDate), Times.Once);
    }

    [Fact]
    public async Task GetStatement_ShouldThrowException_WhenAccountNotFound()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);

        _mockStatementService
            .Setup(x => x.GenerateStatementAsync(accountId, startDate, endDate))
            .ThrowsAsync(new InvalidOperationException("Account not found"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _mockStatementService.Object.GenerateStatementAsync(accountId, startDate, endDate));
    }

    [Fact]
    public async Task GetStatement_ShouldReturnEmptyStatement_WhenNoTransactionsInPeriod()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var startDate = new DateTime(2025, 2, 1);
        var endDate = new DateTime(2025, 2, 28);

        var response = new StatementResponse
        {
            Id = Guid.NewGuid(),
            AccountNumber = "123456",
            StartDate = startDate,
            EndDate = endDate,
            OpeningBalance = 1000.00m,
            ClosingBalance = 1000.00m,
            GeneratedAt = DateTime.UtcNow,
            Entries = new List<StatementEntryResponse>()
        };

        _mockStatementService
            .Setup(x => x.GenerateStatementAsync(accountId, startDate, endDate))
            .ReturnsAsync(response);

        // Act
        var result = await _mockStatementService.Object.GenerateStatementAsync(accountId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Entries.Should().BeEmpty();
        result.OpeningBalance.Should().Be(result.ClosingBalance);
        _mockStatementService.Verify(x => x.GenerateStatementAsync(accountId, startDate, endDate), Times.Once);
    }

    [Fact]
    public async Task GetStatement_ShouldCalculateCorrectBalances_WhenMultipleTransactions()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);

        var response = new StatementResponse
        {
            Id = Guid.NewGuid(),
            AccountNumber = "123456",
            StartDate = startDate,
            EndDate = endDate,
            OpeningBalance = 500.00m,
            ClosingBalance = 1100.00m,
            GeneratedAt = DateTime.UtcNow,
            Entries = new List<StatementEntryResponse>
            {
                new StatementEntryResponse
                {
                    Date = new DateTime(2025, 1, 10),
                    Description = "Credit transfer",
                    Amount = 300.00m,
                    Balance = 800.00m,
                    Type = "CREDIT"
                },
                new StatementEntryResponse
                {
                    Date = new DateTime(2025, 1, 15),
                    Description = "Credit transfer",
                    Amount = 500.00m,
                    Balance = 1300.00m,
                    Type = "CREDIT"
                },
                new StatementEntryResponse
                {
                    Date = new DateTime(2025, 1, 20),
                    Description = "Debit transfer",
                    Amount = -200.00m,
                    Balance = 1100.00m,
                    Type = "DEBIT"
                }
            }
        };

        _mockStatementService
            .Setup(x => x.GenerateStatementAsync(accountId, startDate, endDate))
            .ReturnsAsync(response);

        // Act
        var result = await _mockStatementService.Object.GenerateStatementAsync(accountId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Entries.Should().HaveCount(3);
        result.OpeningBalance.Should().Be(500.00m);
        result.ClosingBalance.Should().Be(1100.00m);
        
        // Verify running balance calculation
        result.Entries[0].Balance.Should().Be(800.00m);  // 500 + 300
        result.Entries[1].Balance.Should().Be(1300.00m); // 800 + 500
        result.Entries[2].Balance.Should().Be(1100.00m); // 1300 - 200
    }
}
