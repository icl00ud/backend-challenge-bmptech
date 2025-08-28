using ChuBank.Domain.Entities;

namespace ChuBank.Domain.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id);
    Task<Account?> GetByAccountNumberAsync(string accountNumber);
    Task<Account> CreateAsync(Account account);
    Task<Account> UpdateAsync(Account account);
    Task<IEnumerable<Account>> GetAllAsync();
}
