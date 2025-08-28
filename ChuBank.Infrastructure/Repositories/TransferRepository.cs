using Microsoft.EntityFrameworkCore;
using ChuBank.Domain.Entities;
using ChuBank.Domain.Interfaces;
using ChuBank.Infrastructure.Data;

namespace ChuBank.Infrastructure.Repositories;

public class TransferRepository : ITransferRepository
{
    private readonly ChuBankDbContext _context;

    public TransferRepository(ChuBankDbContext context)
    {
        _context = context;
    }

    public async Task<Transfer> CreateAsync(Transfer transfer)
    {
        _context.Transfers.Add(transfer);
        await _context.SaveChangesAsync();
        return transfer;
    }

    public async Task<IEnumerable<Transfer>> GetByAccountIdAsync(Guid accountId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Transfers
            .Include(t => t.FromAccount)
            .Include(t => t.ToAccount)
            .Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId);

        if (startDate.HasValue)
            query = query.Where(t => t.TransferDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.TransferDate <= endDate.Value);

        return await query.OrderByDescending(t => t.TransferDate).ToListAsync();
    }

    public async Task<Transfer?> GetByIdAsync(Guid id)
    {
        return await _context.Transfers
            .Include(t => t.FromAccount)
            .Include(t => t.ToAccount)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
}
