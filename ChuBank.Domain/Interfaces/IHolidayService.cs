namespace ChuBank.Domain.Interfaces;

public interface IHolidayService
{
    Task<bool> IsBusinessDayAsync(DateTime date);
}
