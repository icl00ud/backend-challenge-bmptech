using FluentValidation;

namespace ChuBank.Application.DTOs.Requests;

public class CreateTransferRequest
{
    public string FromAccountNumber { get; set; } = string.Empty;
    public string ToAccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}

public class CreateTransferRequestValidator : AbstractValidator<CreateTransferRequest>
{
    public CreateTransferRequestValidator()
    {
        RuleFor(x => x.FromAccountNumber)
            .NotEmpty().WithMessage("Conta de origem é obrigatória");

        RuleFor(x => x.ToAccountNumber)
            .NotEmpty().WithMessage("Conta de destino é obrigatória")
            .NotEqual(x => x.FromAccountNumber).WithMessage("Conta de destino deve ser diferente da conta de origem");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Valor deve ser maior que zero");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Descrição deve ter no máximo 500 caracteres");
    }
}
