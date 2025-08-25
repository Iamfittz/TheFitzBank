using System.ComponentModel.DataAnnotations;

namespace TheFitzBankAPI.Application;
// Request DTOs
public record CreateAccountRequest {
    [Required(ErrorMessage = "Owner name is required")]
    [MinLength(5, ErrorMessage = "Owner name must be at least 5 characters")]
    public string OwnerName { get; init; }

    [Required(ErrorMessage = "Initial balance is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Initial balance cannot be negative")]
    public decimal InitialBalance { get; init; }
}
public record DepositRequest {
    [Required(ErrorMessage = "Account number is required")]
    [MinLength(5, ErrorMessage = "Account number must be at least 5 characters")]
    public string AccountNumber { get; init; }

    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Deposit amount must be greater than zero")]
    public decimal Amount { get; init; }
}
public record TransferRequest {
    [Required(ErrorMessage = "Source account number is required")]
    [MinLength(5, ErrorMessage = "FromAccountNumber must be at least 5 characters")]
    public string FromAccountNumber { get; init; }

    [Required(ErrorMessage = "Destination account number is required")]
    [MinLength(5, ErrorMessage = "ToAccountNumber must be at least 5 characters")]
    [NotEqual(nameof(FromAccountNumber), ErrorMessage = "Cannot transfer to the same account")]
    public string ToAccountNumber { get; init; }

    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Transfer amount must be greater than zero")]
    public decimal Amount { get; init; }
}
public record WithdrawRequest {
    [Required(ErrorMessage = "Account number is required")]
    [MinLength(5, ErrorMessage = "Account number must be at least 5 characters")]
    public string AccountNumber { get; init; }

    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Withdraw amount must be greater than zero")]
    public decimal Amount { get; init; }
}

// Response DTOs
public record AccountResponse(string AccountNumber, string OwnerName, decimal Balance, DateTime CreatedAt, DateTime UpdatedAt);

public record TransferResponse(string FromAccountNumber, string ToAccountNumber, decimal Amount, DateTime TransferredAt, bool Success, string? Message = null);


/// <summary>
/// NotEqualAttribute
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class NotEqualAttribute : ValidationAttribute {
    private readonly string _otherProperty;

    public NotEqualAttribute(string otherProperty) {
        _otherProperty = otherProperty;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) {
        var otherProperty = validationContext.ObjectType.GetProperty(_otherProperty);
        if (otherProperty == null)
            return new ValidationResult($"Unknown property: {_otherProperty}");

        var otherValue = otherProperty.GetValue(validationContext.ObjectInstance);

        if (Equals(value, otherValue))
            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} cannot be the same as {_otherProperty}");

        return ValidationResult.Success;
    }
}


