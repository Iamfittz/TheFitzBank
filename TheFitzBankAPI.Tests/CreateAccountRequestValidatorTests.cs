using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheFitzBankAPI.Application;
using TheFitzBankAPI.Application.Validators;

namespace TheFitzBankAPI.Tests {
    public class CreateAccountRequestValidatorTests {
        [Theory]
        [InlineData("", 0, "must not be empty")]
        [InlineData("Joe", 0, "at least 5 characters")]
        [InlineData("Johnny", -1, "cannot be negative")]
        public void Should_Fail_On_Invalid_Data(string owner, decimal initial, string expectedErrorFragment) {
            var validator = new CreateAccountRequestValidator();
            var req = new CreateAccountRequest(owner, initial);

            var result = validator.Validate(req);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage.Contains(expectedErrorFragment));
        }

        [Fact]
        public void Should_Pass_On_Valid_Data() {
            var validator = new CreateAccountRequestValidator();
            var req = new CreateAccountRequest("Johnny Walker", 100);

            var result = validator.Validate(req);

            Assert.True(result.IsValid);
        }
    }
}
