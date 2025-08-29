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
            .NotEmpty().WithMessage("From Account Number is required");

        RuleFor(x => x.ToAccountNumber)
            .NotEmpty().WithMessage("To Account Number is required")
            .NotEqual(x => x.FromAccountNumber).WithMessage("To Account Number must be different from From Account Number");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must have a maximum of 500 characters");
    }
}
