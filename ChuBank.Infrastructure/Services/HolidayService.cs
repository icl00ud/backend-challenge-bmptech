using ChuBank.Domain.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace ChuBank.Infrastructure.Services;

public class HolidayService : IHolidayService
{
    private readonly HttpClient _httpClient;
    private readonly ICacheService _cacheService;
    private readonly ILogService _logService;

    public HolidayService(HttpClient httpClient, ICacheService cacheService, ILogService logService)
    {
        _httpClient = httpClient;
        _cacheService = cacheService;
        _logService = logService;
    }

    public async Task<bool> IsBusinessDayAsync(DateTime date)
    {
        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            return false;

        var isHoliday = await IsHolidayAsync(date);
        return !isHoliday;
    }

    public async Task WarmCacheAsync()
    {
        var currentYear = DateTime.Now.Year;
        var nextYear = currentYear + 1;
        
        _logService.LogInfo($"Starting holiday cache warm for years {currentYear} and {nextYear}");
        
        await WarmCacheForYearAsync(currentYear);
        await WarmCacheForYearAsync(nextYear);
        
        _logService.LogInfo("Holiday cache warm completed successfully");
    }

    private async Task WarmCacheForYearAsync(int year)
    {
        var cacheKey = $"holidays_{year}";
        
        var cachedHolidays = await _cacheService.GetAsync<List<Holiday>>(cacheKey);
        if (cachedHolidays != null)
        {
            _logService.LogInfo($"Holidays for year {year} already cached");
            return;
        }

        try
        {
            var holidays = await FetchHolidaysFromApiAsync(year);
            var ttl = CalculateCacheTtl(year);
            await _cacheService.SetAsync(cacheKey, holidays, ttl);
            _logService.LogInfo($"Cached {holidays.Count} holidays for year {year}");
        }
        catch (Exception ex)
        {
            _logService.LogError($"Failed to warm cache for holidays year {year}: {ex.Message}");
        }
    }

    private async Task<bool> IsHolidayAsync(DateTime date)
    {
        var year = date.Year;
        var cacheKey = $"holidays_{year}";
        
        var holidays = await _cacheService.GetAsync<List<Holiday>>(cacheKey);
        
        if (holidays == null)
        {
            _logService.LogWarning($"Holiday cache miss for year {year}, fetching from API");
            holidays = await FetchHolidaysFromApiAsync(year);
            var ttl = CalculateCacheTtl(year);
            await _cacheService.SetAsync(cacheKey, holidays, ttl);
        }

        return holidays.Any(h => h.Date.Date == date.Date);
    }

    private TimeSpan CalculateCacheTtl(int year)
    {
        var currentYear = DateTime.Now.Year;
        
        if (year < currentYear)
        {
            return TimeSpan.FromDays(365);
        }
        
        if (year == currentYear)
        {
            var daysUntilEndOfYear = (DateTime.IsLeapYear(year) ? 366 : 365) - DateTime.Now.DayOfYear;
            return TimeSpan.FromDays(daysUntilEndOfYear + 1);
        }

        var endOfYear = new DateTime(year, 12, 31);
        var daysUntilEndOfYearFuture = (endOfYear - DateTime.Now).TotalDays;
        return TimeSpan.FromDays(daysUntilEndOfYearFuture + 1);
    }

    private async Task<List<Holiday>> FetchHolidaysFromApiAsync(int year)
    {
        try
        {
            _logService.LogInfo($"Fetching holidays from BrasilAPI for year {year}");
            var response = await _httpClient.GetStringAsync($"https://brasilapi.com.br/api/feriados/v1/{year}");
            var holidays = JsonSerializer.Deserialize<List<Holiday>>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return holidays ?? new List<Holiday>();
        }
        catch (Exception ex)
        {
            _logService.LogError($"Failed to fetch holidays from BrasilAPI for year {year}: {ex.Message}");
            return new List<Holiday>();
        }
    }

    private class Holiday
    {
        public DateTime Date { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

public class HolidayCacheWarmupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogService _logService;

    public HolidayCacheWarmupService(IServiceProvider serviceProvider, ILogService logService)
    {
        _serviceProvider = serviceProvider;
        _logService = logService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var holidayService = scope.ServiceProvider.GetRequiredService<IHolidayService>();
            await holidayService.WarmCacheAsync();
            
            _logService.LogInfo("Holiday cache warm completed successfully, background service exiting.");
        }
        catch (Exception ex)
        {
            _logService.LogError($"Holiday cache warmup failed: {ex.Message}");
        }
    }
}
