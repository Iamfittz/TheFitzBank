using TheFitzBankAPI.Domain;

namespace TheFitzBankAPI.Tests {
    public class AccountWithdrawTests {
        [Theory]
        [InlineData(100, 10, 90)]
        [InlineData(50.75, 0.75, 50.00)]
        public void Withdraw_ShouldDecreaseBalance(decimal initial, decimal amount, decimal expected) {
            var account = new Account("ACC222", "John Doe", "USD", initial);

            account.Withdraw(amount);

            Assert.Equal(expected, Math.Round(account.Balance, 2));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Withdraw_ShouldThrow_WhenAmountNotPositive(decimal amount) {
            var account = new Account("ACC333", "Jane", "USD", 100);

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => account.Withdraw(amount));

            Assert.Contains("Amount must be > 0", ex.Message);
        }

        [Fact]
        public void Withdraw_ShouldThrow_WhenInsufficientFunds() {
            var account = new Account("ACC444", "Jane", "USD", 10);

            var ex = Assert.Throws<InvalidOperationException>(() => account.Withdraw(100));

            Assert.Equal("Insufficient funds", ex.Message);
        }

        [Fact]
        public void Withdraw_ShouldThrow_WhenAccountClosed() {
            var account = new Account("ACC555", "Mark", "USD", 100);
            account.Close();

            var ex = Assert.Throws<InvalidOperationException>(() => account.Withdraw(1));

            Assert.Equal("Account is closed", ex.Message);
        }
    }
}

