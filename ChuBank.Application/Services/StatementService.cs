using ChuBank.Application.DTOs.Responses;
using ChuBank.Domain.Entities;
using ChuBank.Domain.Interfaces;

namespace ChuBank.Application.Services;

public class StatementService
{
    private readonly IStatementRepository _statementRepository;
    private readonly ITransferRepository _transferRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICacheService _cacheService;

    public StatementService(
        IStatementRepository statementRepository,
        ITransferRepository transferRepository,
        IAccountRepository accountRepository,
        ICacheService cacheService)
    {
        _statementRepository = statementRepository;
        _transferRepository = transferRepository;
        _accountRepository = accountRepository;
        _cacheService = cacheService;
    }

    public async Task<StatementResponse> GenerateStatementAsync(Guid accountId, DateTime startDate, DateTime endDate)
    {
        startDate = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
        endDate = DateTime.SpecifyKind(endDate.Date, DateTimeKind.Utc);
        
        var cacheKey = $"statement_{accountId}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
        
        var cachedStatement = await _cacheService.GetAsync<StatementResponse>(cacheKey);
        if (cachedStatement != null)
            return cachedStatement;

        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null)
            throw new InvalidOperationException("Account not found");

        var transfers = await _transferRepository.GetByAccountIdAsync(accountId, startDate, endDate);

        var openingBalance = await CalculateOpeningBalanceAsync(accountId, startDate);

        var entries = new List<StatementEntryResponse>();
        var runningBalance = openingBalance;

        foreach (var transfer in transfers.OrderBy(t => t.TransferDate))
        {
            var isDebit = transfer.FromAccountId == accountId;
            var amount = isDebit ? -transfer.Amount : transfer.Amount;
            runningBalance += amount;

            entries.Add(new StatementEntryResponse
            {
                Date = transfer.TransferDate,
                Description = transfer.Description ?? (isDebit ? $"Transfer to {transfer.ToAccount.AccountNumber}" : $"Transfer from {transfer.FromAccount.AccountNumber}"),
                Amount = amount,
                Balance = runningBalance,
                Type = isDebit ? "DEBIT" : "CREDIT"
            });
        }

        var statement = new StatementResponse
        {
            Id = Guid.NewGuid(),
            AccountNumber = account.AccountNumber,
            StartDate = startDate,
            EndDate = endDate,
            OpeningBalance = openingBalance,
            ClosingBalance = runningBalance,
            GeneratedAt = DateTime.UtcNow,
            Entries = entries
        };

        await _cacheService.SetAsync(cacheKey, statement, TimeSpan.FromMinutes(30));

        return statement;
    }

    private async Task<decimal> CalculateOpeningBalanceAsync(Guid accountId, DateTime startDate)
    {
        var endDate = DateTime.SpecifyKind(startDate.AddDays(-1), DateTimeKind.Utc);
        var transfers = await _transferRepository.GetByAccountIdAsync(accountId, null, endDate);

        decimal balance = 0;
        
        foreach (var transfer in transfers.OrderBy(t => t.TransferDate))
        {
            if (transfer.FromAccountId == accountId)
                balance -= transfer.Amount;
            else
                balance += transfer.Amount;
        }

        return balance;
    }
}
