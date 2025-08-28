namespace ChuBank.Application.DTOs.Responses;

public class TransferResponse
{
    public Guid Id { get; set; }
    public string FromAccountNumber { get; set; } = string.Empty;
    public string ToAccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime TransferDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
