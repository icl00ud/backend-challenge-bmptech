using FluentAssertions;
using ChuBank.Domain.Entities;

namespace ChuBank.Tests.Domain;

public class EntitiesTests
{
    [Fact]
    public void Account_ShouldCreateWithCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var accountNumber = "123456";
        var holderName = "João Silva";
        var balance = 1000.00m;
        var createdAt = DateTime.UtcNow;

        // Act
        var account = new Account
        {
            Id = id,
            AccountNumber = accountNumber,
            HolderName = holderName,
            Balance = balance,
            CreatedAt = createdAt,
            IsActive = true
        };

        // Assert
        account.Id.Should().Be(id);
        account.AccountNumber.Should().Be(accountNumber);
        account.HolderName.Should().Be(holderName);
        account.Balance.Should().Be(balance);
        account.CreatedAt.Should().Be(createdAt);
        account.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Transfer_ShouldCreateWithCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();
        var amount = 150.00m;
        var description = "Test transfer";
        var transferDate = DateTime.Today;
        var createdAt = DateTime.UtcNow;

        // Act
        var transfer = new Transfer
        {
            Id = id,
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            Amount = amount,
            Description = description,
            TransferDate = transferDate,
            CreatedAt = createdAt
        };

        // Assert
        transfer.Id.Should().Be(id);
        transfer.FromAccountId.Should().Be(fromAccountId);
        transfer.ToAccountId.Should().Be(toAccountId);
        transfer.Amount.Should().Be(amount);
        transfer.Description.Should().Be(description);
        transfer.TransferDate.Should().Be(transferDate);
        transfer.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void Statement_ShouldCreateWithCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);
        var openingBalance = 1000.00m;
        var closingBalance = 800.00m;
        var generatedAt = DateTime.UtcNow;

        // Act
        var statement = new Statement
        {
            Id = id,
            AccountId = accountId,
            StartDate = startDate,
            EndDate = endDate,
            OpeningBalance = openingBalance,
            ClosingBalance = closingBalance,
            GeneratedAt = generatedAt
        };

        // Assert
        statement.Id.Should().Be(id);
        statement.AccountId.Should().Be(accountId);
        statement.StartDate.Should().Be(startDate);
        statement.EndDate.Should().Be(endDate);
        statement.OpeningBalance.Should().Be(openingBalance);
        statement.ClosingBalance.Should().Be(closingBalance);
        statement.GeneratedAt.Should().Be(generatedAt);
    }

    [Fact]
    public void StatementEntry_ShouldCreateWithCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var statementId = Guid.NewGuid();
        var transferId = Guid.NewGuid();
        var date = DateTime.Today;
        var description = "Test transaction";
        var amount = 200.00m;
        var balance = 1200.00m;
        var type = "CREDIT";

        // Act
        var statementEntry = new StatementEntry
        {
            Id = id,
            StatementId = statementId,
            TransferId = transferId,
            Date = date,
            Description = description,
            Amount = amount,
            Balance = balance,
            Type = type
        };

        // Assert
        statementEntry.Id.Should().Be(id);
        statementEntry.StatementId.Should().Be(statementId);
        statementEntry.TransferId.Should().Be(transferId);
        statementEntry.Date.Should().Be(date);
        statementEntry.Description.Should().Be(description);
        statementEntry.Amount.Should().Be(amount);
        statementEntry.Balance.Should().Be(balance);
        statementEntry.Type.Should().Be(type);
    }

    [Fact]
    public void User_ShouldCreateWithCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var username = "test_user";
        var email = "test@example.com";
        var passwordHash = "hashed_password";
        var createdAt = DateTime.UtcNow;

        // Act
        var user = new User
        {
            Id = id,
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = createdAt,
            IsActive = true
        };

        // Assert
        user.Id.Should().Be(id);
        user.Username.Should().Be(username);
        user.Email.Should().Be(email);
        user.PasswordHash.Should().Be(passwordHash);
        user.CreatedAt.Should().Be(createdAt);
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Account_ShouldInitializeCollections()
    {
        // Act
        var account = new Account();

        // Assert
        account.SentTransfers.Should().NotBeNull();
        account.ReceivedTransfers.Should().NotBeNull();
    }

    [Fact]
    public void Statement_ShouldInitializeEntriesCollection()
    {
        // Act
        var statement = new Statement();

        // Assert
        statement.Entries.Should().NotBeNull();
        statement.Entries.Should().BeEmpty();
    }
}
