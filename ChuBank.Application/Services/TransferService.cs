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

    public TransferService(
        ITransferRepository transferRepository,
        IAccountRepository accountRepository,
        IHolidayService holidayService)
    {
        _transferRepository = transferRepository;
        _accountRepository = accountRepository;
        _holidayService = holidayService;
    }

    public async Task<TransferResponse> CreateTransferAsync(CreateTransferRequest request)
    {
        // Validate business day
        var transferDate = DateTime.Today;
        if (!await _holidayService.IsBusinessDayAsync(transferDate))
        {
            throw new InvalidOperationException("Transferências só podem ser realizadas em dias úteis");
        }

        // Get accounts
        var fromAccount = await _accountRepository.GetByAccountNumberAsync(request.FromAccountNumber);
        if (fromAccount == null)
            throw new InvalidOperationException("Conta de origem não encontrada");

        var toAccount = await _accountRepository.GetByAccountNumberAsync(request.ToAccountNumber);
        if (toAccount == null)
            throw new InvalidOperationException("Conta de destino não encontrada");

        // Validate balance
        if (fromAccount.Balance < request.Amount)
            throw new InvalidOperationException("Saldo insuficiente");

        // Update balances
        fromAccount.Balance -= request.Amount;
        toAccount.Balance += request.Amount;

        await _accountRepository.UpdateAsync(fromAccount);
        await _accountRepository.UpdateAsync(toAccount);

        // Create transfer record
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
