using FluentValidation;

namespace ChuBank.Application.DTOs.Requests;

public class CreateAccountRequest
{
    public string HolderName { get; set; } = string.Empty;
    public decimal InitialBalance { get; set; }
}

public class CreateAccountRequestValidator : AbstractValidator<CreateAccountRequest>
{
    public CreateAccountRequestValidator()
    {
        RuleFor(x => x.HolderName)
            .NotEmpty().WithMessage("Nome do titular é obrigatório")
            .Length(2, 200).WithMessage("Nome deve ter entre 2 e 200 caracteres");

        RuleFor(x => x.InitialBalance)
            .GreaterThanOrEqualTo(0).WithMessage("Saldo inicial deve ser maior ou igual a zero");
    }
}
