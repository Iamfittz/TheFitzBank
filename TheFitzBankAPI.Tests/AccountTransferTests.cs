using TheFitzBankAPI.Domain;

namespace TheFitzBankAPI.Tests {
    public class AccountTransferTests {
        [Fact]
        public void TransferTo_ShouldMoveMoney_BetweenSameCurrency() {
            var from = new Account("ACC100", "A", "USD", 100);
            var to = new Account("ACC200", "B", "USD", 10);

            from.TransferTo(to, 40);

            Assert.Equal(60, Math.Round(from.Balance, 2));
            Assert.Equal(50, Math.Round(to.Balance, 2));
        }

        [Fact]
        public void TransferTo_ShouldThrow_WhenDestinationNull() {
            var from = new Account("ACC100", "A", "USD", 100);

            var ex = Assert.Throws<ArgumentNullException>(() => from.TransferTo(null!, 10));

            Assert.Equal("destination", ex.ParamName);
        }

        [Fact]
        public void TransferTo_ShouldThrow_WhenCurrencyMismatch() {
            var from = new Account("ACC100", "A", "USD", 100);
            var to = new Account("ACC200", "B", "EUR", 10);

            var ex = Assert.Throws<InvalidOperationException>(() => from.TransferTo(to, 10));

            Assert.Equal("Cannot transfer between different currencies", ex.Message);
        }

        [Fact]
        public void TransferTo_ShouldThrow_WhenInsufficientFunds() {
            var from = new Account("ACC100", "A", "USD", 5);
            var to = new Account("ACC200", "B", "USD", 10);

            var ex = Assert.Throws<InvalidOperationException>(() => from.TransferTo(to, 100));

            Assert.Equal("Insufficient funds", ex.Message);
        }
    }
}
