using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ChuBank.Application.Services;

namespace ChuBank.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class StatementsController : ControllerBase
{
    private readonly StatementService _statementService;

    public StatementsController(StatementService statementService)
    {
        _statementService = statementService;
    }

    /// <summary>
    /// Gerar extrato de uma conta por período
    /// </summary>
    /// <param name="accountId">ID da conta</param>
    /// <param name="startDate">Data de início do período</param>
    /// <param name="endDate">Data de fim do período</param>
    /// <returns>Extrato da conta no período especificado</returns>
    [HttpGet]
    public async Task<IActionResult> GetStatement(
        [FromQuery] Guid accountId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
            return BadRequest(new { message = "Data de início deve ser anterior à data de fim" });

        if (endDate > DateTime.Today)
            return BadRequest(new { message = "Data de fim não pode ser no futuro" });

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
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
}
