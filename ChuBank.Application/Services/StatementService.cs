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
        var cacheKey = $"statement_{accountId}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
        
        // Try to get from cache first
        var cachedStatement = await _cacheService.GetAsync<StatementResponse>(cacheKey);
        if (cachedStatement != null)
            return cachedStatement;

        // Get account
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null)
            throw new InvalidOperationException("Conta não encontrada");

        // Get transfers in period
        var transfers = await _transferRepository.GetByAccountIdAsync(accountId, startDate, endDate);

        // Calculate opening balance (balance before start date)
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
                Description = transfer.Description ?? (isDebit ? $"Transferência para {transfer.ToAccount.AccountNumber}" : $"Transferência de {transfer.FromAccount.AccountNumber}"),
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

        // Cache for 1 hour
        await _cacheService.SetAsync(cacheKey, statement, TimeSpan.FromHours(1));

        return statement;
    }

    private async Task<decimal> CalculateOpeningBalanceAsync(Guid accountId, DateTime startDate)
    {
        var transfers = await _transferRepository.GetByAccountIdAsync(accountId, null, startDate.AddDays(-1));
        
        // This is a simplified calculation. In a real scenario, you'd have the initial account balance
        // and calculate from there, or store historical balances
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
