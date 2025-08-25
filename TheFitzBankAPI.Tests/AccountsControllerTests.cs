using Microsoft.AspNetCore.Mvc;
using Moq;
using TheFitzBankAPI.Application;
using TheFitzBankAPI.Controller;

namespace TheFitzBankAPI.Tests.Controllers;

public class AccountsControllerTests {
    private readonly Mock<IAccountService> _accountServiceMock;
    private readonly AccountsController _controller;

    public AccountsControllerTests() {
        _accountServiceMock = new Mock<IAccountService>();
        _controller = new AccountsController(_accountServiceMock.Object);
    }

    [Fact]
    public async Task Create_ValidRequest_ShouldReturnCreatedResult() {
        var request = new CreateAccountRequest {
            OwnerName = "Test User",
            InitialBalance = 1000m
        };
        var response = new AccountResponse("ACC123456", "Test User", 1000m, DateTime.UtcNow, DateTime.UtcNow);
        var result = Result<AccountResponse>.Success(response);

        _accountServiceMock.Setup(x => x.CreateAccountAsync(request))
                          .ReturnsAsync(result);

        var actionResult = await _controller.Create(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult);
        Assert.Equal(nameof(_controller.Get), createdResult.ActionName);
        Assert.Equal(response, createdResult.Value);
    }

    [Fact]
    public async Task Create_ServiceFailure_ShouldReturnBadRequest() {
        var request = new CreateAccountRequest {
            OwnerName = "Test User",
            InitialBalance = 1000m
        };
        var result = Result<AccountResponse>.Failure("Validation error");

        _accountServiceMock.Setup(x => x.CreateAccountAsync(request))
                          .ReturnsAsync(result);

        var actionResult = await _controller.Create(request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
        Assert.Equal(result, badRequestResult.Value);
    }

    [Fact]
    public async Task Get_ExistingAccount_ShouldReturnOkResult() {
        var accountNumber = "ACC123456";
        var response = new AccountResponse(accountNumber, "Test User", 1000m, DateTime.UtcNow, DateTime.UtcNow);
        var result = Result<AccountResponse>.Success(response);

        _accountServiceMock.Setup(x => x.GetAccountAsync(accountNumber))
                          .ReturnsAsync(result);

        var actionResult = await _controller.Get(accountNumber);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllAccounts() {
        var accounts = new List<AccountResponse> {
            new("ACC111111", "User 1", 1000m, DateTime.UtcNow, DateTime.UtcNow),
            new("ACC222222", "User 2", 2000m, DateTime.UtcNow, DateTime.UtcNow)
        };
        var result = Result<IReadOnlyList<AccountResponse>>.Success(accounts);

        _accountServiceMock.Setup(x => x.GetAllAccountsAsync())
                          .ReturnsAsync(result);

        var actionResult = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(accounts, okResult.Value);
    }
}

