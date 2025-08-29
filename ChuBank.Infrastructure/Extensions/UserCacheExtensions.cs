using ChuBank.Domain.Entities;
using ChuBank.Infrastructure.DTOs;

namespace ChuBank.Infrastructure.Extensions;

public static class UserCacheExtensions
{
    public static UserCacheDto ToCacheDto(this User user)
    {
        return new UserCacheDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            IsLocked = user.IsLocked,
            FailedLoginAttempts = user.FailedLoginAttempts,
            LastLoginAt = user.LastLoginAt,
            LockedUntil = user.LockedUntil,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            RoleNames = user.UserRoles?.Select(ur => ur.Role?.Name ?? string.Empty).Where(name => !string.IsNullOrEmpty(name)).ToList() ?? new List<string>()
        };
    }

    public static User ToUser(this UserCacheDto cacheDto)
    {
        return new User
        {
            Id = cacheDto.Id,
            Username = cacheDto.Username,
            Email = cacheDto.Email,
            PasswordHash = cacheDto.PasswordHash,
            FirstName = cacheDto.FirstName,
            LastName = cacheDto.LastName,
            IsActive = cacheDto.IsActive,
            IsLocked = cacheDto.IsLocked,
            FailedLoginAttempts = cacheDto.FailedLoginAttempts,
            LastLoginAt = cacheDto.LastLoginAt,
            LockedUntil = cacheDto.LockedUntil,
            CreatedAt = cacheDto.CreatedAt,
            UpdatedAt = cacheDto.UpdatedAt,
            UserRoles = new List<UserRole>()
        };
    }
}
