using Microsoft.EntityFrameworkCore;
using ChuBank.Domain.Entities;
using ChuBank.Domain.Interfaces;
using ChuBank.Infrastructure.Data;

namespace ChuBank.Infrastructure.Repositories;

public class StatementRepository : IStatementRepository
{
    private readonly ChuBankDbContext _context;

    public StatementRepository(ChuBankDbContext context)
    {
        _context = context;
    }

    public async Task<Statement> CreateAsync(Statement statement)
    {
        _context.Statements.Add(statement);
        await _context.SaveChangesAsync();
        return statement;
    }

    public async Task<Statement?> GetByAccountIdAndPeriodAsync(Guid accountId, DateTime startDate, DateTime endDate)
    {
        return await _context.Statements
            .Include(s => s.Entries)
            .ThenInclude(e => e.Transfer)
            .FirstOrDefaultAsync(s => s.AccountId == accountId && 
                               s.StartDate == startDate && 
                               s.EndDate == endDate);
    }
}
