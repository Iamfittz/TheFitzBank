using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheFitzBankAPI.Application;
using TheFitzBankAPI.Application.Validators;

namespace TheFitzBankAPI.Tests {
    public class DepositRequestValidatorTests {
        [Theory]
        [InlineData("", 50, "Account number is required")]
        [InlineData("ACC1", 0, "greater than zero")]
        [InlineData("ACC1", -5, "greater than zero")]
        public void Should_Fail_On_Invalid_Data(string acc, decimal amount, string expectedErrorFragment) {
            var validator = new DepositRequestValidator();
            var request = new DepositRequest(acc, amount);

            var result = validator.Validate(request);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage.Contains(expectedErrorFragment));
        }

        [Fact]
        public void Should_Pass_On_Valid_Data() {
            var validator = new DepositRequestValidator();
            var request = new DepositRequest("ACC42", 10);

            var result = validator.Validate(request);

            Assert.True(result.IsValid);
        }
    }
}
