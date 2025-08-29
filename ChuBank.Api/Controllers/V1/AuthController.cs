using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ChuBank.Domain.Interfaces;
using ChuBank.Application.DTOs.Requests;
using ChuBank.Application.DTOs.Responses;

namespace ChuBank.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogService _logService;

    public AuthController(IAuthService authService, ILogService logService)
    {
        _authService = authService;
        _logService = logService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        var (user, token) = await _authService.AuthenticateAsync(
            request.Username, 
            request.Password, 
            ipAddress, 
            userAgent);

        if (user == null || token == null)
        {
            return Unauthorized(new { message = "Invalid credentials or account locked" });
        }

        var response = new AuthResponse
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(8),
            User = new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
                LastLoginAt = user.LastLoginAt,
                CreatedAt = user.CreatedAt
            }
        };

        return Ok(response);
    }

    [HttpPost("register")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Register([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var user = await _authService.CreateUserAsync(
                request.Username,
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                request.Roles);

            _logService.LogInfo($"User registration successful: {request.Username}");

            var response = new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
                CreatedAt = user.CreatedAt
            };

            return Created($"api/v1/auth/{user.Id}", response);
        }
        catch (InvalidOperationException ex)
        {
            _logService.LogWarning($"User registration failed: {request.Username} - {ex.Message}");
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized();
        }

        var user = await _authService.GetUserByIdAsync(userGuid);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var response = new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        };

        return Ok(response);
    }
}
