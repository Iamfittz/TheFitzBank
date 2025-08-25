namespace TheFitzBankAPI.Domain;

public sealed class Account {
    public int Id { get; private set; }
    public string AccountNumber { get; private set; } = string.Empty;
    public string OwnerName { get; private set; } = string.Empty;
    public decimal Balance { get; private set; }
    public string Currency { get; private set; } = "USD";

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsClosed { get; private set; }
    public DateTime? ClosedAt { get; private set; }

    // CTOR EF
    private Account() { }

    public Account(string accountNumber, string ownerName, string currency, decimal initialBalance) {
        if (string.IsNullOrWhiteSpace(accountNumber)) throw new ArgumentException("Empty account number", nameof(accountNumber));
        if (string.IsNullOrWhiteSpace(ownerName)) throw new ArgumentException("Empty owner name", nameof(ownerName));
        if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Empty currency", nameof(currency));
        if (initialBalance < 0) throw new ArgumentOutOfRangeException(nameof(initialBalance), "Initial balance cannot be negative");

        AccountNumber = accountNumber.Trim();
        OwnerName = ownerName.Trim();
        Currency = currency.Trim().ToUpperInvariant();
        Balance = decimal.Round(initialBalance, 2, MidpointRounding.ToZero);
    }

    public void Deposit(decimal amount) {
        Balance = decimal.Round(Balance + amount, 2, MidpointRounding.ToZero);
        Touch();
    }

    public void Withdraw(decimal amount) {
        Balance = decimal.Round(Balance - amount, 2, MidpointRounding.ToZero);
        Touch();
    }

    public void TransferTo(Account destination, decimal amount) {
        Withdraw(amount);
        destination.Deposit(amount);
    }

    public void Close() {
        if (IsClosed) return;
        IsClosed = true;
        ClosedAt = DateTime.UtcNow;
        Touch();
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;
}
