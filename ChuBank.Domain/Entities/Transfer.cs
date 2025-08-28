namespace ChuBank.Domain.Entities;

public class Transfer
{
    public Guid Id { get; set; }
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime TransferDate { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public Account FromAccount { get; set; } = null!;
    public Account ToAccount { get; set; } = null!;
}
