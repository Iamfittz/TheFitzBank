using FluentValidation;

namespace TheFitzBankAPI.Application.Validators {
    public class TransferRequestValidator : AbstractValidator<TransferRequest> {
        public TransferRequestValidator() {
            RuleFor(x => x.FromAccountNumber)
                .NotEmpty()
                .WithMessage("Source account number is required");

            RuleFor(x => x.ToAccountNumber)
                .NotEmpty()
                .WithMessage("Account number is required")
                .NotEqual(x => x.FromAccountNumber)
                .WithMessage("Cannot transfer to the same account");

            RuleFor(x => x.Amount)
                .NotEmpty()
                .GreaterThan(0)
                .WithMessage("Transfer amount must be greater than zero");
        }
    }
}
