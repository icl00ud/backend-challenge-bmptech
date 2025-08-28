using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ChuBank.Application.Services;
using ChuBank.Application.DTOs.Requests;
using FluentValidation;

namespace ChuBank.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly AccountService _accountService;
    private readonly IValidator<CreateAccountRequest> _validator;

    public AccountsController(AccountService accountService, IValidator<CreateAccountRequest> validator)
    {
        _accountService = accountService;
        _validator = validator;
    }

    /// <summary>
    /// Cadastrar uma nova conta
    /// </summary>
    /// <param name="request">Dados da conta a ser criada</param>
    /// <returns>Dados da conta criada</returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        try
        {
            var account = await _accountService.CreateAccountAsync(request);
            return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, account);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Obter dados de uma conta por ID
    /// </summary>
    /// <param name="id">ID da conta</param>
    /// <returns>Dados da conta</returns>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetAccount(Guid id)
    {
        var account = await _accountService.GetAccountByIdAsync(id);
        
        if (account == null)
            return NotFound(new { message = "Conta não encontrada" });

        return Ok(account);
    }
}
