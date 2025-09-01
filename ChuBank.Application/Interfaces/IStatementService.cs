using ChuBank.Application.DTOs.Responses;

namespace ChuBank.Application.Interfaces;

public interface IStatementService
{
    Task<StatementResponse> GenerateStatementAsync(Guid accountId, DateTime startDate, DateTime endDate);
}
