using ChuBank.Domain.Entities;

namespace ChuBank.Domain.Interfaces;

public interface ITransferRepository
{
    Task<Transfer> CreateAsync(Transfer transfer);
    Task<IEnumerable<Transfer>> GetByAccountIdAsync(Guid accountId, DateTime? startDate = null, DateTime? endDate = null);
    Task<Transfer?> GetByIdAsync(Guid id);
}
