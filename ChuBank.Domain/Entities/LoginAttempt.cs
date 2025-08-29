namespace ChuBank.Domain.Entities;

public class LoginAttempt
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public string? FailureReason { get; set; }
    public DateTime AttemptedAt { get; set; }
    
    public User User { get; set; } = null!;
}
