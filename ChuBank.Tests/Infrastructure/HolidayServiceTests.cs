using Moq;
using FluentAssertions;
using ChuBank.Infrastructure.Services;
using ChuBank.Domain.Interfaces;
using System.Net;
using Moq.Protected;

namespace ChuBank.Tests.Infrastructure;

public class HolidayServiceTests
{
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<ILogService> _mockLogService;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly HolidayService _holidayService;

    public HolidayServiceTests()
    {
        _mockCacheService = new Mock<ICacheService>();
        _mockLogService = new Mock<ILogService>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _holidayService = new HolidayService(_httpClient, _mockCacheService.Object, _mockLogService.Object);
    }

    [Fact]
    public async Task IsBusinessDayAsync_ShouldReturnFalse_WhenDateIsSaturday()
    {
        // Arrange
        var saturday = new DateTime(2025, 1, 4); // Saturday

        // Act
        var result = await _holidayService.IsBusinessDayAsync(saturday);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsBusinessDayAsync_ShouldReturnFalse_WhenDateIsSunday()
    {
        // Arrange
        var sunday = new DateTime(2025, 1, 5); // Sunday

        // Act
        var result = await _holidayService.IsBusinessDayAsync(sunday);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task WarmCacheAsync_ShouldLogStartAndCompletion()
    {
        // Arrange
        var currentYear = DateTime.Now.Year;
        var nextYear = currentYear + 1;

        // Setup HTTP to return empty response (no holidays)
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[]")
            });

        // Act
        await _holidayService.WarmCacheAsync();

        // Assert
        _mockLogService.Verify(x => x.LogInfo($"Starting holiday cache warm for years {currentYear} and {nextYear}"), Times.Once);
        _mockLogService.Verify(x => x.LogInfo("Holiday cache warm completed successfully"), Times.Once);
    }
}
