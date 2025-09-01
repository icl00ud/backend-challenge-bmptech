using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ChuBank.Application.Interfaces;

namespace ChuBank.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class StatementsController : ControllerBase
{
    private readonly IStatementService _statementService;

    public StatementsController(IStatementService statementService)
    {
        _statementService = statementService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStatement(
        [FromQuery] Guid accountId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
            return BadRequest(new { message = "Start date must be before end date" });

        if (endDate > DateTime.Today)
            return BadRequest(new { message = "End date cannot be in the future" });

        try
        {
            var statement = await _statementService.GenerateStatementAsync(accountId, startDate, endDate);
            return Ok(statement);
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
