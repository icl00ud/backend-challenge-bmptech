using ChuBank.Domain.Interfaces;

namespace ChuBank.Infrastructure.Services;

public class LogService : ILogService
{
    public void LogInfo(string message)
    {
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] [INFO] {message}");
    }

    public void LogWarning(string message)
    {
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] [WARN] {message}");
    }

    public void LogError(string message)
    {
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] [ERROR] {message}");
    }

    public void LogSecurity(string message)
    {
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] [SECURITY] {message}");
    }
}
