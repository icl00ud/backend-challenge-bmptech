using ChuBank.Application.DTOs.Requests;
using ChuBank.Application.DTOs.Responses;

namespace ChuBank.Application.Interfaces;

public interface IAccountService
{
    Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request);
    Task<AccountResponse?> GetAccountByIdAsync(Guid id);
    Task<IEnumerable<AccountResponse>> GetAllAccountsAsync();
}
