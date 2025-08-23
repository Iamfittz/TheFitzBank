namespace TheFitzBankAPI.Application;
// Request DTOs
public record CreateAccountRequest(string OwnerName, decimal InitialBalance);

public record DepositRequest(string AccountNumber, decimal Amount);

public record TransferRequest(string FromAccountNumber, string ToAccountNumber, decimal Amount);

// Response DTOs
public record AccountResponse(string AccountNumber, string OwnerName, decimal Balance, DateTime CreatedAt, DateTime UpdatedAt);

public record TransferResponse(string FromAccountNumber, string ToAccountNumber, decimal Amount, DateTime TransferredAt, bool Success, string? Message = null);

public record OperationResponse(bool Success, string Message, object? Data = null);

public record WithdrawRequest(string AccountNumber, decimal Amount);

