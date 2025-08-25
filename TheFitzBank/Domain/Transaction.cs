namespace TheFitzBankAPI.Domain {
    public class Transaction {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string? FromAccount { get; set; }
        public string? ToAccount { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
