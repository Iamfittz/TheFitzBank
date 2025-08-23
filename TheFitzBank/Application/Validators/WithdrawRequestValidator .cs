using FluentValidation;

namespace TheFitzBankAPI.Application.Validators {
    public class WithdrawRequestValidator : AbstractValidator<WithdrawRequest> {
        public WithdrawRequestValidator() {
            RuleFor(x => x.AccountNumber)
                .NotEmpty()
                .WithMessage("Account number is required");

            RuleFor(x => x.Amount)
                .NotEmpty()
                .GreaterThan(0)
                .WithMessage("Withdraw amount must be greater than zero");
        }
    }
}
