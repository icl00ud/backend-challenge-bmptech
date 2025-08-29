using ChuBank.Domain.Entities;

namespace ChuBank.Domain.Interfaces;

public interface IAuthService
{
    Task<(User? user, string? token)> AuthenticateAsync(string username, string password, string ipAddress, string userAgent);
    Task<User> CreateUserAsync(string username, string email, string password, string firstName, string lastName, IEnumerable<string> roleNames);
    Task<User?> GetUserByIdAsync(Guid userId);
    Task LogLoginAttemptAsync(Guid userId, string ipAddress, string userAgent, bool isSuccessful, string? failureReason = null);
    Task<bool> IsUserLockedAsync(Guid userId);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}
