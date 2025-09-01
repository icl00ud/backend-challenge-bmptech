using Moq;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using ChuBank.Infrastructure.Services;
using System.Text;
using System.Text.Json;

namespace ChuBank.Tests.Infrastructure;

public class CacheServiceTests
{
    private readonly Mock<IDistributedCache> _mockDistributedCache;
    private readonly CacheService _cacheService;

    public CacheServiceTests()
    {
        _mockDistributedCache = new Mock<IDistributedCache>();
        _cacheService = new CacheService(_mockDistributedCache.Object);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnObject_WhenCacheHit()
    {
        // Arrange
        var key = "test_key";
        var testObject = new TestData { Id = 1, Name = "Test" };
        var serializedValue = JsonSerializer.Serialize(testObject);
        var valueBytes = Encoding.UTF8.GetBytes(serializedValue);

        _mockDistributedCache
            .Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(valueBytes);

        // Act
        var result = await _cacheService.GetAsync<TestData>(key);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(testObject.Id);
        result.Name.Should().Be(testObject.Name);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenCacheMiss()
    {
        // Arrange
        var key = "missing_key";

        _mockDistributedCache
            .Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _cacheService.GetAsync<TestData>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenCacheValueIsEmpty()
    {
        // Arrange
        var key = "empty_key";
        var emptyBytes = Array.Empty<byte>();

        _mockDistributedCache
            .Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyBytes);

        // Act
        var result = await _cacheService.GetAsync<TestData>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_ShouldSetValueWithCustomExpiry_WhenExpiryProvided()
    {
        // Arrange
        var key = "test_key";
        var testObject = new TestData { Id = 1, Name = "Test" };
        var expiry = TimeSpan.FromMinutes(60);
        var expectedSerializedValue = JsonSerializer.Serialize(testObject);
        var expectedBytes = Encoding.UTF8.GetBytes(expectedSerializedValue);

        // Act
        await _cacheService.SetAsync(key, testObject, expiry);

        // Assert
        _mockDistributedCache.Verify(
            x => x.SetAsync(
                key,
                It.Is<byte[]>(bytes => 
                    Encoding.UTF8.GetString(bytes) == expectedSerializedValue),
                It.Is<DistributedCacheEntryOptions>(opts => 
                    opts.AbsoluteExpirationRelativeToNow == expiry),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SetAsync_ShouldSetValueWithDefaultExpiry_WhenNoExpiryProvided()
    {
        // Arrange
        var key = "test_key";
        var testObject = new TestData { Id = 1, Name = "Test" };
        var expectedSerializedValue = JsonSerializer.Serialize(testObject);

        // Act
        await _cacheService.SetAsync(key, testObject);

        // Assert
        _mockDistributedCache.Verify(
            x => x.SetAsync(
                key,
                It.Is<byte[]>(bytes => 
                    Encoding.UTF8.GetString(bytes) == expectedSerializedValue),
                It.Is<DistributedCacheEntryOptions>(opts => 
                    opts.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(30)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_ShouldCallDistributedCacheRemove()
    {
        // Arrange
        var key = "test_key";

        // Act
        await _cacheService.RemoveAsync(key);

        // Assert
        _mockDistributedCache.Verify(
            x => x.RemoveAsync(key, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SetAsync_ShouldSerializeObjectCorrectly()
    {
        // Arrange
        var key = "test_key";
        var testObject = new TestData { Id = 42, Name = "Complex Test Object" };
        var expectedJson = JsonSerializer.Serialize(testObject);

        // Act
        await _cacheService.SetAsync(key, testObject);

        // Assert
        _mockDistributedCache.Verify(
            x => x.SetAsync(
                key,
                It.Is<byte[]>(bytes => 
                    Encoding.UTF8.GetString(bytes) == expectedJson),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
