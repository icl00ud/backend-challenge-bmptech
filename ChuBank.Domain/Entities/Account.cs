namespace ChuBank.Domain.Entities;

public class Account
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string HolderName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    public ICollection<Transfer> SentTransfers { get; set; } = new List<Transfer>();
    public ICollection<Transfer> ReceivedTransfers { get; set; } = new List<Transfer>();
}
