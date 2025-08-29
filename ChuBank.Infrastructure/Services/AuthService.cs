using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChuBank.Domain.Entities;
using ChuBank.Domain.Interfaces;
using ChuBank.Infrastructure.DTOs;
using ChuBank.Infrastructure.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace ChuBank.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogService _logService;
    private readonly ICacheService _cacheService;
    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 30;
    private const int RateLimitWindow = 15;
    private const int MaxAttemptsPerIp = 10;

    public AuthService(
        IUserRepository userRepository, 
        IRoleRepository roleRepository,
        IConfiguration configuration,
        ILogService logService,
        ICacheService cacheService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _configuration = configuration;
        _logService = logService;
        _cacheService = cacheService;
    }

    public async Task<(User? user, string? token)> AuthenticateAsync(string username, string password, string ipAddress, string userAgent)
    {
        _logService.LogSecurity($"Login attempt for user: {username} from IP: {ipAddress}");
        
        // Check IP rate limiting
        if (await IsIpRateLimitedAsync(ipAddress))
        {
            _logService.LogSecurity($"Login attempt blocked - IP rate limited: {ipAddress}");
            return (null, null);
        }

        // Try to get user from cache first
        var cacheKey = $"user_{username}";
        var cachedUserDto = await _cacheService.GetAsync<UserCacheDto>(cacheKey);
        User? user = null;
        
        if (cachedUserDto != null)
        {
            // Convert cached DTO back to User entity
            user = cachedUserDto.ToUser();
            // For cached users, we need to get a fresh instance from DB for role information and tracking
            var freshUser = await _userRepository.GetByUsernameAsync(username);
            if (freshUser != null)
            {
                // Copy role information to our cached user
                user.UserRoles = freshUser.UserRoles;
                // Use the fresh user for any updates to avoid tracking conflicts
                user = freshUser;
            }
            _logService.LogInfo($"User retrieved from cache: {username}");
        }
        else
        {
            user = await _userRepository.GetByUsernameAsync(username);
            if (user != null)
            {
                // Cache user for 15 minutes
                await _cacheService.SetAsync(cacheKey, user.ToCacheDto(), TimeSpan.FromMinutes(15));
                _logService.LogInfo($"User retrieved from database and cached: {username}");
            }
        }
        
        if (user == null)
        {
            await RecordFailedLoginAttemptAsync(ipAddress);
            _logService.LogSecurity($"Login failed - user not found: {username} from IP: {ipAddress}");
            return (null, null);
        }

        if (await IsUserLockedAsync(user.Id))
        {
            await RecordFailedLoginAttemptAsync(ipAddress);
            await LogLoginAttemptAsync(user.Id, ipAddress, userAgent, false, "Account locked");
            _logService.LogSecurity($"Login failed - account locked: {username} from IP: {ipAddress}");
            return (null, null);
        }

        if (!VerifyPassword(password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;
            
            if (user.FailedLoginAttempts >= MaxFailedAttempts)
            {
                user.IsLocked = true;
                user.LockedUntil = DateTime.UtcNow.AddMinutes(LockoutMinutes);
                _logService.LogSecurity($"Account locked due to failed attempts: {username} from IP: {ipAddress}");
            }
            
            await _userRepository.UpdateAsync(user);
            
            // Update cache with modified user data
            await _cacheService.SetAsync(cacheKey, user.ToCacheDto(), TimeSpan.FromMinutes(15));
            
            await LogLoginAttemptAsync(user.Id, ipAddress, userAgent, false, "Invalid password");
            await RecordFailedLoginAttemptAsync(ipAddress);
            _logService.LogSecurity($"Login failed - invalid password: {username} from IP: {ipAddress}");
            return (null, null);
        }

        // Reset failed attempts on successful login
        user.FailedLoginAttempts = 0;
        user.LastLoginAt = DateTime.UtcNow;
        user.IsLocked = false;
        user.LockedUntil = null;
        
        await _userRepository.UpdateAsync(user);
        await LogLoginAttemptAsync(user.Id, ipAddress, userAgent, true);

        // Update user cache with fresh data
        await _cacheService.SetAsync(cacheKey, user.ToCacheDto(), TimeSpan.FromMinutes(15));
        _logService.LogInfo($"Updated user cache after successful login: {username}");

        var token = GenerateJwtToken(user);
        _logService.LogSecurity($"Login successful: {username} from IP: {ipAddress}");
        return (user, token);
    }

    public async Task<User> CreateUserAsync(string username, string email, string password, string firstName, string lastName, IEnumerable<string> roleNames)
    {
        if (await _userRepository.ExistsAsync(username, email))
        {
            _logService.LogWarning($"User creation failed - username or email already exists: {username}, {email}");
            throw new InvalidOperationException("Username or email already exists");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            PasswordHash = HashPassword(password),
            FirstName = firstName,
            LastName = lastName,
            IsActive = true
        };

        user = await _userRepository.CreateAsync(user);

        foreach (var roleName in roleNames)
        {
            var role = await _roleRepository.GetByNameAsync(roleName);
            if (role != null)
            {
                user.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id,
                    AssignedAt = DateTime.UtcNow
                });
            }
        }

        await _userRepository.UpdateAsync(user);
        _logService.LogInfo($"User created successfully: {username} with roles: {string.Join(", ", roleNames)}");
        return user;
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }

    public Task LogLoginAttemptAsync(Guid userId, string ipAddress, string userAgent, bool isSuccessful, string? failureReason = null)
    {
        return Task.CompletedTask;
    }

    public async Task<bool> IsUserLockedAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        if (!user.IsLocked || !user.LockedUntil.HasValue) return false;

        if (user.LockedUntil <= DateTime.UtcNow)
        {
            user.IsLocked = false;
            user.LockedUntil = null;
            user.FailedLoginAttempts = 0;
            await _userRepository.UpdateAsync(user);
            
            // Update cache with auto-unlocked user data
            var cacheKey = $"user_{user.Username}";
            await _cacheService.SetAsync(cacheKey, user.ToCacheDto(), TimeSpan.FromMinutes(15));
            
            _logService.LogInfo($"User auto-unlocked after lockout period: {user.Username}");
            return false;
        }

        return true;
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new("FirstName", user.FirstName),
            new("LastName", user.LastName)
        };

        foreach (var userRole in user.UserRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<bool> IsIpRateLimitedAsync(string ipAddress)
    {
        var cacheKey = $"login_attempts_{ipAddress}";
        var attemptsWrapper = await _cacheService.GetAsync<LoginAttemptCounter>(cacheKey);
        
        if (attemptsWrapper != null && attemptsWrapper.Count >= MaxAttemptsPerIp)
        {
            _logService.LogSecurity($"IP rate limited: {ipAddress} - {attemptsWrapper.Count} attempts");
            return true;
        }
        
        return false;
    }

    private async Task RecordFailedLoginAttemptAsync(string ipAddress)
    {
        var cacheKey = $"login_attempts_{ipAddress}";
        var attemptsWrapper = await _cacheService.GetAsync<LoginAttemptCounter>(cacheKey);
        var attempts = attemptsWrapper?.Count ?? 0;
        attempts++;
        
        await _cacheService.SetAsync(cacheKey, new LoginAttemptCounter { Count = attempts }, TimeSpan.FromMinutes(RateLimitWindow));
        _logService.LogInfo($"Recorded failed login attempt for IP: {ipAddress} (total: {attempts})");
    }
}

public class LoginAttemptCounter
{
    public int Count { get; set; }
}
