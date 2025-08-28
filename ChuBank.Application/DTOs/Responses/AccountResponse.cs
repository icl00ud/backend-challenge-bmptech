namespace ChuBank.Application.DTOs.Responses;

public class AccountResponse
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string HolderName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
