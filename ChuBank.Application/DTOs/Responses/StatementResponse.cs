namespace ChuBank.Application.DTOs.Responses;

public class StatementResponse
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public DateTime GeneratedAt { get; set; }
    public List<StatementEntryResponse> Entries { get; set; } = new();
}

public class StatementEntryResponse
{
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public string Type { get; set; } = string.Empty;
}
