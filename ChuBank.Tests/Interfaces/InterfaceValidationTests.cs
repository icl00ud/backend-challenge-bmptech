using Moq;
using FluentAssertions;
using ChuBank.Domain.Interfaces;

namespace ChuBank.Tests.Interfaces;

public class InterfaceValidationTests
{
    [Fact]
    public void IAccountRepository_ShouldHaveCorrectMethods()
    {
        // Arrange & Act
        var mock = new Mock<IAccountRepository>();
        var accountId = Guid.NewGuid();
        var accountNumber = "123456";

        // Assert - Verify interface methods exist and can be mocked
        mock.Setup(x => x.GetByIdAsync(accountId)).Verifiable();
        mock.Setup(x => x.GetByAccountNumberAsync(accountNumber)).Verifiable();
        mock.Setup(x => x.CreateAsync(It.IsAny<ChuBank.Domain.Entities.Account>())).Verifiable();
        mock.Setup(x => x.UpdateAsync(It.IsAny<ChuBank.Domain.Entities.Account>())).Verifiable();

        // Test passes if no exceptions are thrown
        Assert.True(true);
    }

    [Fact]
    public void ITransferRepository_ShouldHaveCorrectMethods()
    {
        // Arrange & Act
        var mock = new Mock<ITransferRepository>();
        var accountId = Guid.NewGuid();
        var startDate = DateTime.Today;
        var endDate = DateTime.Today.AddDays(30);

        // Assert - Verify interface methods exist and can be mocked
        mock.Setup(x => x.CreateAsync(It.IsAny<ChuBank.Domain.Entities.Transfer>())).Verifiable();
        mock.Setup(x => x.GetByAccountIdAsync(accountId, startDate, endDate)).Verifiable();

        // Test passes if no exceptions are thrown
        Assert.True(true);
    }

    [Fact]
    public void IStatementRepository_ShouldHaveCorrectMethods()
    {
        // Arrange & Act
        var mock = new Mock<IStatementRepository>();
        var accountId = Guid.NewGuid();
        var startDate = DateTime.Today;
        var endDate = DateTime.Today.AddDays(30);

        // Assert - Verify interface methods exist and can be mocked
        mock.Setup(x => x.CreateAsync(It.IsAny<ChuBank.Domain.Entities.Statement>())).Verifiable();
        mock.Setup(x => x.GetByAccountIdAndPeriodAsync(accountId, startDate, endDate)).Verifiable();

        // Test passes if no exceptions are thrown
        Assert.True(true);
    }

    [Fact]
    public void ICacheService_ShouldHaveCorrectMethods()
    {
        // Arrange & Act
        var mock = new Mock<ICacheService>();
        var key = "test_key";
        var value = "test_value";
        var expiry = TimeSpan.FromMinutes(30);

        // Assert - Verify interface methods exist and can be mocked
        mock.Setup(x => x.GetAsync<string>(key)).Verifiable();
        mock.Setup(x => x.SetAsync(key, value, expiry)).Verifiable();
        mock.Setup(x => x.RemoveAsync(key)).Verifiable();

        // Test passes if no exceptions are thrown
        Assert.True(true);
    }

    [Fact]
    public void IHolidayService_ShouldHaveCorrectMethods()
    {
        // Arrange & Act
        var mock = new Mock<IHolidayService>();
        var date = DateTime.Today;

        // Assert - Verify interface methods exist and can be mocked
        mock.Setup(x => x.IsBusinessDayAsync(date)).Verifiable();
        mock.Setup(x => x.WarmCacheAsync()).Verifiable();

        // Test passes if no exceptions are thrown
        Assert.True(true);
    }

    [Fact]
    public void ILogService_ShouldHaveCorrectMethods()
    {
        // Arrange & Act
        var mock = new Mock<ILogService>();
        var message = "Test message";

        // Assert - Verify interface methods exist and can be mocked
        mock.Setup(x => x.LogInfo(message)).Verifiable();
        mock.Setup(x => x.LogWarning(message)).Verifiable();
        mock.Setup(x => x.LogError(message)).Verifiable();

        // Test passes if no exceptions are thrown
        Assert.True(true);
    }
}
