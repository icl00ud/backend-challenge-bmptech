using ChuBank.Domain.Interfaces;
using System.Text.Json;

namespace ChuBank.Infrastructure.Services;

public class HolidayService : IHolidayService
{
    private readonly HttpClient _httpClient;
    private readonly ICacheService _cacheService;

    public HolidayService(HttpClient httpClient, ICacheService cacheService)
    {
        _httpClient = httpClient;
        _cacheService = cacheService;
    }

    public async Task<bool> IsBusinessDayAsync(DateTime date)
    {
        // Weekend check
        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            return false;

        // Check if it's a holiday
        var isHoliday = await IsHolidayAsync(date);
        return !isHoliday;
    }

    private async Task<bool> IsHolidayAsync(DateTime date)
    {
        var year = date.Year;
        var cacheKey = $"holidays_{year}";
        
        var holidays = await _cacheService.GetAsync<List<Holiday>>(cacheKey);
        
        if (holidays == null)
        {
            holidays = await FetchHolidaysFromApiAsync(year);
            await _cacheService.SetAsync(cacheKey, holidays, TimeSpan.FromDays(1));
        }

        return holidays.Any(h => h.Date.Date == date.Date);
    }

    private async Task<List<Holiday>> FetchHolidaysFromApiAsync(int year)
    {
        try
        {
            var response = await _httpClient.GetStringAsync($"https://brasilapi.com.br/api/feriados/v1/{year}");
            var holidays = JsonSerializer.Deserialize<List<Holiday>>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return holidays ?? new List<Holiday>();
        }
        catch
        {
            // If API fails, return empty list (fail safe)
            return new List<Holiday>();
        }
    }

    private class Holiday
    {
        public DateTime Date { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
