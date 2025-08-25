using Microsoft.AspNetCore.Mvc;
using Moq;
using TheFitzBankAPI.Application;
using TheFitzBankAPI.Controller;

namespace TheFitzBankAPI.Tests.Controllers;

public class TransactionControllerTests {
    private readonly Mock<IAccountService> _accountServiceMock;
    private readonly TransactionController _controller;

    public TransactionControllerTests() {
        _accountServiceMock = new Mock<IAccountService>();
        _controller = new TransactionController(_accountServiceMock.Object);
    }

    [Fact]
    public async Task Deposit_ValidRequest_ShouldReturnOkResult() {
        var request = new DepositRequest {
            AccountNumber = "ACC123456",
            Amount = 500m
        };
        var response = new AccountResponse("ACC123456", "Test User", 1500m, DateTime.UtcNow, DateTime.UtcNow);
        var result = Result<AccountResponse>.Success(response);

        _accountServiceMock.Setup(x => x.DepositAsync(request))
                          .ReturnsAsync(result);

        var actionResult = await _controller.Deposit(request);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(result, okResult.Value);
    }

    [Fact]
    public async Task Transfer_ValidRequest_ShouldReturnOkResult() {
        var request = new TransferRequest {
            FromAccountNumber = "ACC111111",
            ToAccountNumber = "ACC222222",
            Amount = 300m
        };
        var response = new TransferResponse("ACC111111", "ACC222222", 300m, DateTime.UtcNow, true, "Success");
        var result = Result<TransferResponse>.Success(response);

        _accountServiceMock.Setup(x => x.TransferAsync(request))
                          .ReturnsAsync(result);

        var actionResult = await _controller.Transfer(request);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(result, okResult.Value);
    }

    [Fact]
    public async Task Withdraw_ValidRequest_ShouldReturnOkResult() {
        var request = new WithdrawRequest {
            AccountNumber = "ACC123456",
            Amount = 300m
        };
        var result = Result.Success();

        _accountServiceMock.Setup(x => x.WithdrawAsync(request))
                          .ReturnsAsync(result);

        var actionResult = await _controller.Withdraw(request);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(result, okResult.Value);
    }
}