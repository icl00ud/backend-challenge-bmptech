using ChuBank.Application.DTOs.Requests;
using ChuBank.Application.DTOs.Responses;
using ChuBank.Domain.Entities;
using ChuBank.Domain.Interfaces;

namespace ChuBank.Application.Services;

public class AccountService
{
    private readonly IAccountRepository _accountRepository;

    public AccountService(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request)
    {
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

        return new AccountResponse
        {
            Id = createdAccount.Id,
            AccountNumber = createdAccount.AccountNumber,
            HolderName = createdAccount.HolderName,
            Balance = createdAccount.Balance,
            CreatedAt = createdAccount.CreatedAt,
            IsActive = createdAccount.IsActive
        };
    }

    public async Task<AccountResponse?> GetAccountByIdAsync(Guid id)
    {
        var account = await _accountRepository.GetByIdAsync(id);
        
        if (account == null)
            return null;

        return new AccountResponse
        {
            Id = account.Id,
            AccountNumber = account.AccountNumber,
            HolderName = account.HolderName,
            Balance = account.Balance,
            CreatedAt = account.CreatedAt,
            IsActive = account.IsActive
        };
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
