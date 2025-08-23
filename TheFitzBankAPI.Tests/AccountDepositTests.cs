using TheFitzBankAPI.Domain;

namespace TheFitzBankAPI.Tests {
    public class AccountDepositTests {
        //Ok
        [Theory]
        [InlineData(100, 50, 150)]
        [InlineData(200, 25.5, 225.5)]
        [InlineData(0, 10, 10)]
        public void Deposit_ShouldIncreaseBalance(decimal initial, decimal amount, decimal expected) {
            var account = new Account("ACC123456", "John Doe", "USD", initial);

            account.Deposit(amount);

            Assert.Equal(expected, Math.Round(account.Balance, 2));
        }

        [Theory]
        [InlineData(100.001, 0.01, 100.01)]
        [InlineData(200.555, 0.004, 200.55)]
        public void Deposit_ShouldRound_ToTwoDecimals(decimal initial, decimal amount, decimal expected) {
            var account = new Account("ACC777", "Bob", "USD", initial);

            account.Deposit(amount);

            Assert.Equal(expected, Math.Round(account.Balance, 2));
        }


        // Exceptions: Invalid Sum
        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        [InlineData(-100)]
        public void Deposit_ShouldThrow_WhenAmountIsNotPositive(decimal amount) {
            var account = new Account("ACC123456", "John Doe", "USD", 100);

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => account.Deposit(amount));

            Assert.Contains("Amount must be > 0", ex.Message);
        }

        // Exceptions: Closed Acc
        [Fact]
        public void Deposit_ShouldThrow_WhenAccountClosed() {
            var account = new Account("ACC999", "Alice", "USD", 100);
            account.Close();

            var ex = Assert.Throws<InvalidOperationException>(() => account.Deposit(50));

            Assert.Equal("Account is closed", ex.Message);
        }
    }
}
