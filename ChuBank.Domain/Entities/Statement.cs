namespace ChuBank.Domain.Entities;

public class Statement
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public DateTime GeneratedAt { get; set; }
    
    public Account Account { get; set; } = null!;
    public ICollection<StatementEntry> Entries { get; set; } = new List<StatementEntry>();
}
