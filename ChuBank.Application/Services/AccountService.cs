using ChuBank.Application.DTOs.Requests;
using ChuBank.Application.DTOs.Responses;
using ChuBank.Application.Interfaces;
using ChuBank.Domain.Entities;
using ChuBank.Domain.Interfaces;

namespace ChuBank.Application.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogService _logService;
    private readonly ICacheService _cacheService;

    public AccountService(IAccountRepository accountRepository, ILogService logService, ICacheService cacheService)
    {
        _accountRepository = accountRepository;
        _logService = logService;
        _cacheService = cacheService;
    }

    public async Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request)
    {
        _logService.LogInfo($"Creating account for holder: {request.HolderName}");
        
        var accountNumber = await GenerateUniqueAccountNumberAsync();

        var account = new Account
        {
            Id = Guid.NewGuid(),
            AccountNumber = accountNumber,
            HolderName = request.HolderName,
            Balance = request.InitialBalance,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var createdAccount = await _accountRepository.CreateAsync(account);
        _logService.LogInfo($"Account created successfully: {accountNumber} for {request.HolderName}");

        var cacheKey = $"account_{createdAccount.Id}";
        var accountResponse = new AccountResponse
        {
            Id = createdAccount.Id,
            AccountNumber = createdAccount.AccountNumber,
            HolderName = createdAccount.HolderName,
            Balance = createdAccount.Balance,
            CreatedAt = createdAccount.CreatedAt,
            IsActive = createdAccount.IsActive
        };
        
        await _cacheService.SetAsync(cacheKey, accountResponse, TimeSpan.FromMinutes(30));
        
        return accountResponse;
    }

    public async Task<AccountResponse?> GetAccountByIdAsync(Guid id)
    {
        var cacheKey = $"account_{id}";
        var cachedAccount = await _cacheService.GetAsync<AccountResponse>(cacheKey);
        
        if (cachedAccount != null)
        {
            _logService.LogInfo($"Account retrieved from cache: {id}");
            return cachedAccount;
        }

        var account = await _accountRepository.GetByIdAsync(id);
        
        if (account == null)
            return null;

        var accountResponse = new AccountResponse
        {
            Id = account.Id,
            AccountNumber = account.AccountNumber,
            HolderName = account.HolderName,
            Balance = account.Balance,
            CreatedAt = account.CreatedAt,
            IsActive = account.IsActive
        };

        await _cacheService.SetAsync(cacheKey, accountResponse, TimeSpan.FromMinutes(15));
        _logService.LogInfo($"Account retrieved from database and cached: {id}");
        
        return accountResponse;
    }

    public async Task<IEnumerable<AccountResponse>> GetAllAccountsAsync()
    {
        var accounts = await _accountRepository.GetAllAsync();
        
        return accounts.Select(account => new AccountResponse
        {
            Id = account.Id,
            AccountNumber = account.AccountNumber,
            HolderName = account.HolderName,
            Balance = account.Balance,
            CreatedAt = account.CreatedAt,
            IsActive = account.IsActive
        });
    }

    private async Task<string> GenerateUniqueAccountNumberAsync()
    {
        string accountNumber;
        Account? existingAccount;

        do
        {
            accountNumber = Random.Shared.Next(100000, 999999).ToString();
            existingAccount = await _accountRepository.GetByAccountNumberAsync(accountNumber);
        }
        while (existingAccount != null);

        return accountNumber;
    }
}
