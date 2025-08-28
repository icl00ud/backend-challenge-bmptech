using Microsoft.EntityFrameworkCore;
using ChuBank.Domain.Entities;
using ChuBank.Domain.Interfaces;
using ChuBank.Infrastructure.Data;

namespace ChuBank.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly ChuBankDbContext _context;

    public AccountRepository(ChuBankDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByIdAsync(Guid id)
    {
        return await _context.Accounts.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Account?> GetByAccountNumberAsync(string accountNumber)
    {
        return await _context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
    }

    public async Task<Account> CreateAsync(Account account)
    {
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task<Account> UpdateAsync(Account account)
    {
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task<IEnumerable<Account>> GetAllAsync()
    {
        return await _context.Accounts.ToListAsync();
    }
}
