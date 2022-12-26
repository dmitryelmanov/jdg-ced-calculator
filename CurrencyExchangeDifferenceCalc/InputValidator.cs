using FluentValidation;
using FluentValidation.Results;

namespace CurrencyExchangeDifferenceCalc;

internal sealed class InputValidator : AbstractValidator<Input?>
{
    public InputValidator()
    {
        RuleFor(x => x!.Currency)
            .NotEmpty();
        RuleFor(x => x!.Amount)
            .GreaterThan(0);
        RuleFor(x => x!.SellingDate)
            .NotEmpty();
        RuleFor(x => x!.IncomeDate)
            .NotEmpty();
        RuleFor(x => x!.SellingDate)
            .NotEqual(x => x!.IncomeDate)
            .WithMessage($"'{nameof(Input.SellingDate)}' must not be equal '{nameof(Input.IncomeDate)}'");
    }

    public override ValidationResult Validate(ValidationContext<Input?> context)
    {
        if (context.InstanceToValidate is null)
        {
            return new ValidationResult(new[] { new ValidationFailure(string.Empty, "Object is null")});
        }

        return base.Validate(context);
    }
}
