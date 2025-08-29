using ChuBank.Application.DTOs.Requests;
using ChuBank.Application.DTOs.Responses;
using ChuBank.Domain.Entities;
using ChuBank.Domain.Interfaces;

namespace ChuBank.Application.Services;

public class TransferService
{
    private readonly ITransferRepository _transferRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IHolidayService _holidayService;
    private readonly ILogService _logService;

    public TransferService(
        ITransferRepository transferRepository,
        IAccountRepository accountRepository,
        IHolidayService holidayService,
        ILogService logService)
    {
        _transferRepository = transferRepository;
        _accountRepository = accountRepository;
        _holidayService = holidayService;
        _logService = logService;
    }

    public async Task<TransferResponse> CreateTransferAsync(CreateTransferRequest request)
    {
        _logService.LogInfo($"Transfer request: {request.Amount:C} from {request.FromAccountNumber} to {request.ToAccountNumber}");
        
        var transferDate = DateTime.UtcNow.Date;
        if (!await _holidayService.IsBusinessDayAsync(transferDate))
        {
            _logService.LogWarning($"Transfer rejected - not a business day: {transferDate:yyyy-MM-dd}");
            throw new InvalidOperationException("Transfers can only be made on business days");
        }

        var fromAccount = await _accountRepository.GetByAccountNumberAsync(request.FromAccountNumber);
        if (fromAccount == null)
        {
            _logService.LogWarning($"Transfer failed - source account not found: {request.FromAccountNumber}");
            throw new InvalidOperationException("Source account not found");
        }

        var toAccount = await _accountRepository.GetByAccountNumberAsync(request.ToAccountNumber);
        if (toAccount == null)
        {
            _logService.LogWarning($"Transfer failed - destination account not found: {request.ToAccountNumber}");
            throw new InvalidOperationException("Destination account not found");
        }

        if (fromAccount.Balance < request.Amount)
        {
            _logService.LogWarning($"Transfer failed - insufficient balance: {fromAccount.AccountNumber} has {fromAccount.Balance:C}, requested {request.Amount:C}");
            throw new InvalidOperationException("Insufficient funds");
        }

        fromAccount.Balance -= request.Amount;
        toAccount.Balance += request.Amount;

        await _accountRepository.UpdateAsync(fromAccount);
        await _accountRepository.UpdateAsync(toAccount);

        _logService.LogInfo($"Updated balances for accounts: {fromAccount.AccountNumber} and {toAccount.AccountNumber}");

        var transfer = new Transfer
        {
            Id = Guid.NewGuid(),
            FromAccountId = fromAccount.Id,
            ToAccountId = toAccount.Id,
            Amount = request.Amount,
            Description = request.Description,
            TransferDate = transferDate,
            CreatedAt = DateTime.UtcNow
        };

        var createdTransfer = await _transferRepository.CreateAsync(transfer);
        _logService.LogInfo($"Transfer completed successfully: {request.Amount:C} from {request.FromAccountNumber} to {request.ToAccountNumber} - ID: {createdTransfer.Id}");

        return new TransferResponse
        {
            Id = createdTransfer.Id,
            FromAccountNumber = request.FromAccountNumber,
            ToAccountNumber = request.ToAccountNumber,
            Amount = createdTransfer.Amount,
            Description = createdTransfer.Description,
            TransferDate = createdTransfer.TransferDate,
            CreatedAt = createdTransfer.CreatedAt
        };
    }
}
