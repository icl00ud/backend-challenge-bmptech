using ChuBank.Domain.Entities;

namespace ChuBank.Domain.Interfaces;

public interface IStatementRepository
{
    Task<Statement> CreateAsync(Statement statement);
    Task<Statement?> GetByAccountIdAndPeriodAsync(Guid accountId, DateTime startDate, DateTime endDate);
}
