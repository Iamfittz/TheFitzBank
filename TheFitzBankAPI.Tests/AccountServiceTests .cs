using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TheFitzBankAPI.Application;
using TheFitzBankAPI.Application.Services;
using TheFitzBankAPI.Domain;
using TheFitzBankAPI.Infrastructure;

namespace TheFitzBankAPI.Tests.Services;

public class AccountServiceTests : IDisposable {
    private readonly BankingContext _context;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<AccountService>> _loggerMock;
    private readonly AccountService _accountService;

    public AccountServiceTests() {
        var options = new DbContextOptionsBuilder<BankingContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new BankingContext(options);

        var config = new MapperConfiguration(cfg => {
            cfg.CreateMap<Account, AccountResponse>();
        });
        _mapper = config.CreateMapper();

        _loggerMock = new Mock<ILogger<AccountService>>();

        _accountService = new AccountService(_mapper, _loggerMock.Object, _context);
    }

    public void Dispose() {
        _context.Dispose();
    }

    #region CreateAccountAsync Tests

    [Fact]
    public async Task CreateAccountAsync_ValidRequest_ShouldReturnSuccessResult() {
        var request = new CreateAccountRequest {
            OwnerName = "Artem ARVU",
            InitialBalance = 1000m
        };

        var result = await _accountService.CreateAccountAsync(request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Artem ARVU", result.Data.OwnerName);
        Assert.Equal(1000m, result.Data.Balance);
        Assert.StartsWith("ACC", result.Data.AccountNumber);

        var accountInDb = await _context.Accounts.FirstOrDefaultAsync();
        Assert.NotNull(accountInDb);
        Assert.Equal("Artem ARVU", accountInDb.OwnerName);
    }

    [Fact]
    public async Task CreateAccountAsync_ValidRequest_ShouldGenerateUniqueAccountNumber() {
        var request1 = new CreateAccountRequest { OwnerName = "Artem ARVU", InitialBalance = 1000m };
        var request2 = new CreateAccountRequest { OwnerName = "Artem ARVU", InitialBalance = 500m };

        var result1 = await _accountService.CreateAccountAsync(request1);
        var result2 = await _accountService.CreateAccountAsync(request2);

        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.NotEqual(result1.Data.AccountNumber, result2.Data.AccountNumber);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100.50)]
    [InlineData(50)]
    public async Task CreateAccountAsync_DifferentInitialBalances_ShouldCreateAccountSuccessfully(decimal initialBalance) {
        var request = new CreateAccountRequest {
            OwnerName = "Test User",
            InitialBalance = initialBalance
        };

        var result = await _accountService.CreateAccountAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(initialBalance, result.Data.Balance);
    }

    #endregion

    #region GetAccountAsync Tests

    [Fact]
    public async Task GetAccountAsync_ExistingAccount_ShouldReturnAccount() {
        var account = new Account("ACC123456", "Artem ARVU", "USD", 1000m);
        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();

        var result = await _accountService.GetAccountAsync("ACC123456");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("ACC123456", result.Data.AccountNumber);
        Assert.Equal("Artem ARVU", result.Data.OwnerName);
        Assert.Equal(1000m, result.Data.Balance);
    }

    [Fact]
    public async Task GetAccountAsync_NonExistingAccount_ShouldReturnFailure() {
        var result = await _accountService.GetAccountAsync("NONEXISTENT");

        Assert.False(result.IsSuccess);
        Assert.Equal("Account not found", result.ErrorMessage);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task GetAccountAsync_InvalidAccountNumber_ShouldReturnFailure(string accountNumber) {
        var result = await _accountService.GetAccountAsync(accountNumber);

        Assert.False(result.IsSuccess);
    }

    #endregion

    #region GetAllAccountsAsync Tests

    [Fact]
    public async Task GetAllAccountsAsync_NoAccounts_ShouldReturnEmptyList() {
        // Act
        var result = await _accountService.GetAllAccountsAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task GetAllAccountsAsync_MultipleAccounts_ShouldReturnAllAccounts() {
        var accounts = new[]
        {
            new Account("ACC111111", "Artem ARVU", "USD", 1000m),
            new Account("ACC222222", "Robert DeNiro", "USD", 2000m),
            new Account("ACC333333", "Nicolas Jackson", "USD", 1500m)
        };

        await _context.Accounts.AddRangeAsync(accounts);
        await _context.SaveChangesAsync();

        var result = await _accountService.GetAllAccountsAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Data.Count);
        Assert.Contains(result.Data, a => a.AccountNumber == "ACC111111");
        Assert.Contains(result.Data, a => a.AccountNumber == "ACC222222");
        Assert.Contains(result.Data, a => a.AccountNumber == "ACC333333");
    }

    #endregion

    #region DepositAsync Tests

    [Fact]
    public async Task DepositAsync_ValidDeposit_ShouldIncreaseBalance() {
        var account = new Account("ACC123456", "Artem ARVU", "USD", 1000m);
        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();

        var request = new DepositRequest {
            AccountNumber = "ACC123456",
            Amount = 500m
        };

        var result = await _accountService.DepositAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(1500m, result.Data.Balance);

        var updatedAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == "ACC123456");
        Assert.Equal(1500m, updatedAccount.Balance);
    }

    [Fact]
    public async Task DepositAsync_NonExistingAccount_ShouldReturnFailure() {
        var request = new DepositRequest {
            AccountNumber = "NONEXISTENT",
            Amount = 500m
        };

        var result = await _accountService.DepositAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Account not found", result.ErrorMessage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task DepositAsync_InvalidAmount_ShouldHandleGracefully(decimal amount) {

        var account = new Account("ACC123456", "Artem ARVU", "USD", 1000m);

        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();

        var request = new DepositRequest {
            AccountNumber = "ACC123456",
            Amount = amount
        };

        var result = await _accountService.DepositAsync(request);
    }

    #endregion

    #region TransferAsync Tests

    [Fact]
    public async Task TransferAsync_ValidTransfer_ShouldTransferFunds() {
        var fromAccount = new Account("ACC111111", "Artem ARVU", "USD", 1000m);
        var toAccount = new Account("ACC222222", "Jane Smith", "USD", 500m);

        await _context.Accounts.AddRangeAsync(fromAccount, toAccount);
        await _context.SaveChangesAsync();

        var request = new TransferRequest {
            FromAccountNumber = "ACC111111",
            ToAccountNumber = "ACC222222",
            Amount = 300m
        };

        var result = await _accountService.TransferAsync(request);

        Assert.True(result.IsSuccess);
        Assert.True(result.Data.Success);
        Assert.Equal("Transfer successful", result.Data.Message);

        var updatedFromAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == "ACC111111");
        var updatedToAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == "ACC222222");

        Assert.Equal(700m, updatedFromAccount.Balance);
        Assert.Equal(800m, updatedToAccount.Balance);
    }

    [Fact]
    public async Task TransferAsync_NonExistingFromAccount_ShouldReturnFailureResponse() {
        var toAccount = new Account("ACC222222", "Jane Smith", "USD", 500m);
        await _context.Accounts.AddAsync(toAccount);
        await _context.SaveChangesAsync();

        var request = new TransferRequest {
            FromAccountNumber = "NONEXISTENT",
            ToAccountNumber = "ACC222222",
            Amount = 300m
        };

        var result = await _accountService.TransferAsync(request);

        Assert.True(result.IsSuccess);
        Assert.False(result.Data.Success);
        Assert.Equal("One or both accounts not found", result.Data.Message);
    }

    [Fact]
    public async Task TransferAsync_NonExistingToAccount_ShouldReturnFailureResponse() {
        var fromAccount = new Account("ACC111111", "Artem ARVU", "USD", 1000m);
        await _context.Accounts.AddAsync(fromAccount);
        await _context.SaveChangesAsync();

        var request = new TransferRequest {
            FromAccountNumber = "ACC111111",
            ToAccountNumber = "NONEXISTENT",
            Amount = 300m
        };

        var result = await _accountService.TransferAsync(request);

        Assert.True(result.IsSuccess);
        Assert.False(result.Data.Success);
        Assert.Equal("One or both accounts not found", result.Data.Message);
    }

    [Fact]
    public async Task TransferAsync_InsufficientFunds_ShouldReturnFailureResponse() {
        var fromAccount = new Account("ACC111111", "Artem ARVU", "USD", 100m);
        var toAccount = new Account("ACC222222", "Nicolas Jackson", "USD", 500m);

        await _context.Accounts.AddRangeAsync(fromAccount, toAccount);
        await _context.SaveChangesAsync();

        var request = new TransferRequest {
            FromAccountNumber = "ACC111111",
            ToAccountNumber = "ACC222222",
            Amount = 200m
        };

        var result = await _accountService.TransferAsync(request);

        Assert.False(result.IsSuccess);
    }

    #endregion

    #region WithdrawAsync Tests

    [Fact]
    public async Task WithdrawAsync_ValidWithdraw_ShouldDecreaseBalance() {
        var account = new Account("ACC123456", "Artem ARVU", "USD", 1000m);
        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();

        var request = new WithdrawRequest {
            AccountNumber = "ACC123456",
            Amount = 300m
        };

        var result = await _accountService.WithdrawAsync(request);

        Assert.True(result.IsSuccess);

        var updatedAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == "ACC123456");
        Assert.Equal(700m, updatedAccount.Balance);
    }

    [Fact]
    public async Task WithdrawAsync_NonExistingAccount_ShouldReturnFailure() {
        var request = new WithdrawRequest {
            AccountNumber = "NONEXISTENT",
            Amount = 300m
        };

        var result = await _accountService.WithdrawAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Account not found", result.ErrorMessage);
    }

    [Fact]
    public async Task WithdrawAsync_InsufficientFunds_ShouldReturnFailure() {
        var account = new Account("ACC123456", "Artem ARVU", "USD", 100m);
        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();

        var request = new WithdrawRequest {
            AccountNumber = "ACC123456",
            Amount = 200m
        };

        var result = await _accountService.WithdrawAsync(request);

        Assert.False(result.IsSuccess);
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task CreateAccountAsync_ShouldLogInformationMessages() {
        var request = new CreateAccountRequest {
            OwnerName = "Artem ARVU",
            InitialBalance = 1000m
        };

        await _accountService.CreateAccountAsync(request);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CreateAccount requested")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Account created")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAccountAsync_NonExistingAccount_ShouldLogWarning() {
        await _accountService.GetAccountAsync("NONEXISTENT");

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Account not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion
}