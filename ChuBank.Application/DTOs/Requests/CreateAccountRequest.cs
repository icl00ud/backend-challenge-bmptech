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
            .NotEmpty().WithMessage("Holder name is required")
            .Length(2, 200).WithMessage("Name must be between 2 and 200 characters");

        RuleFor(x => x.InitialBalance)
            .GreaterThanOrEqualTo(0).WithMessage("Initial balance must be greater than or equal to zero");
    }
}
