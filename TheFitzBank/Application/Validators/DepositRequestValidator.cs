using FluentValidation;

namespace TheFitzBankAPI.Application.Validators {
    public class DepositRequestValidator : AbstractValidator<DepositRequest> {
        public DepositRequestValidator() {
            RuleFor(x => x.AccountNumber)
                .NotEmpty()
                .WithMessage("Account number is required");

            RuleFor(x => x.Amount)
                .NotEmpty()
                .GreaterThan(0)
                .WithMessage("Deposit amount must be greater than zero");
        }
    }
}
