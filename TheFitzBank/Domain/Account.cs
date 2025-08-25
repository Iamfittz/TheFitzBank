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
        EnsureActive();
        EnsurePositive(amount);

        Balance = decimal.Round(Balance + amount, 2, MidpointRounding.ToZero);
        Touch();
    }

    public void Withdraw(decimal amount) {
        EnsureActive();
        EnsurePositive(amount);

        if (Balance < amount)
            throw new InvalidOperationException("Insufficient funds");

        Balance = decimal.Round(Balance - amount, 2, MidpointRounding.ToZero);
        Touch();
    }

    public void TransferTo(Account destination, decimal amount) {
        if (destination is null) throw new ArgumentNullException(nameof(destination));
        if (!string.Equals(Currency, destination.Currency, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Cannot transfer between different currencies");

        Withdraw(amount);
        destination.Deposit(amount);
    }

    public void Close() {
        if (IsClosed) return;
        IsClosed = true;
        ClosedAt = DateTime.UtcNow;
        Touch();
    }

    private static void EnsurePositive(decimal amount) {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be > 0");
    }

    private void EnsureActive() {
        if (IsClosed) throw new InvalidOperationException("Account is closed");
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;
}
