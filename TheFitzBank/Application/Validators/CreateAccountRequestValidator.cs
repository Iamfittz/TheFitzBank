using FluentValidation;

namespace TheFitzBankAPI.Application.Validators {
    public class CreateAccountRequestValidator : AbstractValidator<CreateAccountRequest> {
        public CreateAccountRequestValidator() {
            RuleFor(x => x.OwnerName)
                .NotEmpty()
                .MinimumLength(5)
                .WithMessage("Owner name must be at least 5 characters");

            RuleFor(x => x.InitialBalance)
                .NotEmpty()
                .GreaterThanOrEqualTo(0)
                .WithMessage("Initial balance cannot be negative");
        }
    }
}
