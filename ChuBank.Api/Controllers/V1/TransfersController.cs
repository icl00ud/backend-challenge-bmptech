using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ChuBank.Application.Services;
using ChuBank.Application.DTOs.Requests;
using FluentValidation;

namespace ChuBank.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class TransfersController : ControllerBase
{
    private readonly TransferService _transferService;
    private readonly IValidator<CreateTransferRequest> _validator;

    public TransfersController(TransferService transferService, IValidator<CreateTransferRequest> validator)
    {
        _transferService = transferService;
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTransfer([FromBody] CreateTransferRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        try
        {
            var transfer = await _transferService.CreateTransferAsync(request);
            return Ok(transfer);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
            catch (Exception)
            {
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
