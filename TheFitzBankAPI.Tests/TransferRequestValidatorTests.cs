using TheFitzBankAPI.Application;
using TheFitzBankAPI.Application.Validators;

namespace TheFitzBankAPI.Tests {
    public class TransferRequestValidatorTests {
        [Fact]
        public void Should_Fail_When_To_Same_As_From() {
            var validator = new TransferRequestValidator();
            var req = new TransferRequest("A", "A", 10);

            var result = validator.Validate(req);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Cannot transfer to the same account"));
        }

        [Theory]
        [InlineData("", "B", 10, "Source account number is required")]
        [InlineData("A", "", 10, "Account number is required")]
        [InlineData("A", "B", 0, "greater than zero")]
        public void Should_Fail_On_Invalid_Data(string from, string to, decimal amount, string expectedErrorFragment) {
            var validator = new TransferRequestValidator();
            var req = new TransferRequest(from, to, amount);

            var result = validator.Validate(req);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage.Contains(expectedErrorFragment));
        }

        [Fact]
        public void Should_Pass_On_Valid_Data() {
            var validator = new TransferRequestValidator();
            var req = new TransferRequest("A1", "B1", 10);

            var result = validator.Validate(req);

            Assert.True(result.IsValid);
        }
    }
}
