namespace ChuBank.Domain.Entities;

public class StatementEntry
{
    public Guid Id { get; set; }
    public Guid StatementId { get; set; }
    public Guid? TransferId { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public string Type { get; set; } = string.Empty; // "CREDIT" or "DEBIT"
    
    public Statement Statement { get; set; } = null!;
    public Transfer? Transfer { get; set; }
}
