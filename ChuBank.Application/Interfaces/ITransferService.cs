using ChuBank.Application.DTOs.Requests;
using ChuBank.Application.DTOs.Responses;

namespace ChuBank.Application.Interfaces;

public interface ITransferService
{
    Task<TransferResponse> CreateTransferAsync(CreateTransferRequest request);
}
