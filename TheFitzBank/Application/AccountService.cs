using AutoMapper;
using TheFitzBankAPI.Application.Validators;
using TheFitzBankAPI.Domain;

namespace TheFitzBankAPI.Application.Services;

public sealed class AccountService : IAccountService {
    private readonly IAccountRepository _accountRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AccountService> _logger;
    public AccountService(IAccountRepository accountRepository, IMapper mapper, ILogger<AccountService> logger) {
        _accountRepository = accountRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<OperationResponse> CreateAccountAsync(CreateAccountRequest request) {
        _logger.LogInformation("CreateAccount requested: Owner={Owner}, InitialBalance={Balance}",
            request.OwnerName, request.InitialBalance);

        // todo: Replace validator instant with DI if validators gain dependencies.
        var validator = new CreateAccountRequestValidator();
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid) {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("CreateAccount validation failed: Owner={Owner}, Errors={Errors}",
                request.OwnerName, errors);
            return new OperationResponse(false, $"Validation failed: {errors}");
        }

        string accNumber = "ACC" + Random.Shared.Next(100000, 999999);

        var account = new Account(
            accNumber,
            request.OwnerName,
            "USD",
            request.InitialBalance);

        await _accountRepository.AddAsync(account);

        _logger.LogInformation("Account created: AccountNumber={AccountNumber}, Owner={Owner}, Balance={Balance}",
            account.AccountNumber, account.OwnerName, account.Balance);

        var response = _mapper.Map<AccountResponse>(account);
        return new OperationResponse(true, "Account created successfully", response);
    }



    public async Task<AccountResponse?> GetAccountAsync(string accountNumber) {
        _logger.LogInformation("GetAccount requested: AccountNumber={AccountNumber}", accountNumber);

        var account = await _accountRepository.GetByAccountNumberAsync(accountNumber);

        if (account == null) {
            _logger.LogWarning("Account not found: AccountNumber={AccountNumber}", accountNumber);
            return null;
        }

        _logger.LogInformation("Account found: AccountNumber={AccountNumber}, Owner={Owner}, Balance={Balance}",
            account.AccountNumber, account.OwnerName, account.Balance);

        return _mapper.Map<AccountResponse>(account);
    }


    public async Task<IReadOnlyList<AccountResponse>> GetAllAccountsAsync() {
        _logger.LogInformation("GetAllAccounts requested");

        var accounts = await _accountRepository.GetAllAsync();

        _logger.LogInformation("Accounts retrieved: Count={Count}", accounts.Count);

        return _mapper.Map<IReadOnlyList<AccountResponse>>(accounts);
    }


    public async Task<OperationResponse> DepositAsync(DepositRequest request) {
        _logger.LogInformation("Deposit requested: Account={AccountNumber}, Amount={Amount}",
            request.AccountNumber, request.Amount);

        var validator = new DepositRequestValidator();
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid) {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Deposit validation failed for Account={AccountNumber}: {Errors}",
                request.AccountNumber, errors);
            return new OperationResponse(false, $"Validation failed: {errors}");
        }

        var account = await _accountRepository.GetByAccountNumberAsync(request.AccountNumber);
        if (account == null) {
            _logger.LogWarning("Deposit failed: account not found: {AccountNumber}", request.AccountNumber);
            return new OperationResponse(false, "Account not found");
        }

        account.Deposit(request.Amount);
        await _accountRepository.UpdateAsync(account);

        _logger.LogInformation("Deposit successful: Account={AccountNumber}, NewBalance={Balance}",
            account.AccountNumber, account.Balance);

        var response = _mapper.Map<AccountResponse>(account);
        return new OperationResponse(true, "Deposit successful", response);
    }


    public async Task<TransferResponse> TransferAsync(TransferRequest request) {
        _logger.LogInformation("Transfer requested: From={From}, To={To}, Amount={Amount}",
            request.FromAccountNumber, request.ToAccountNumber, request.Amount);

        var validator = new TransferRequestValidator();
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid) {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Transfer validation failed: From={From}, To={To}, Errors={Errors}",
                request.FromAccountNumber, request.ToAccountNumber, errors);

            return new TransferResponse(request.FromAccountNumber, request.ToAccountNumber,
                request.Amount, DateTime.UtcNow, false, $"Validation failed: {errors}");
        }

        var from = await _accountRepository.GetByAccountNumberAsync(request.FromAccountNumber);
        var to = await _accountRepository.GetByAccountNumberAsync(request.ToAccountNumber);

        if (from == null || to == null) {
            _logger.LogWarning("Transfer failed: One or both accounts not found. From={From}, To={To}",
                request.FromAccountNumber, request.ToAccountNumber);

            return new TransferResponse(request.FromAccountNumber, request.ToAccountNumber,
                request.Amount, DateTime.UtcNow, false, "One or both accounts not found");
        }

        try {
            from.TransferTo(to, request.Amount);
            await _accountRepository.UpdateAsync(from);
            await _accountRepository.UpdateAsync(to);

            _logger.LogInformation("Transfer successful: From={From}, To={To}, Amount={Amount}",
                request.FromAccountNumber, request.ToAccountNumber, request.Amount);

            return new TransferResponse(request.FromAccountNumber, request.ToAccountNumber,
                request.Amount, DateTime.UtcNow, true, "Transfer successful");
        } catch (Exception ex) {
            _logger.LogError(ex, "Transfer failed with exception: From={From}, To={To}, Amount={Amount}",
                request.FromAccountNumber, request.ToAccountNumber, request.Amount);

            return new TransferResponse(request.FromAccountNumber, request.ToAccountNumber,
                request.Amount, DateTime.UtcNow, false, ex.Message);
        }
    }

    public async Task<OperationResponse> WithdrawAsync(WithdrawRequest request) {
        _logger.LogInformation("Withdraw requested: Account={AccountNumber}, Amount={Amount}",
            request.AccountNumber, request.Amount);

        var account = await _accountRepository.GetByAccountNumberAsync(request.AccountNumber);
        if (account == null) {
            _logger.LogWarning("Withdraw failed: Account not found: {AccountNumber}", request.AccountNumber);
            return new OperationResponse(false, "Account not found");
        }

        try {
            account.Withdraw(request.Amount);
            await _accountRepository.UpdateAsync(account);

            _logger.LogInformation("Withdraw successful: Account={AccountNumber}, NewBalance={Balance}",
                account.AccountNumber, account.Balance);

            return new OperationResponse(true, $"Withdrawn {request.Amount} from account {account.AccountNumber}");
        } catch (Exception ex) {
            _logger.LogError(ex, "Withdraw failed with exception: Account={AccountNumber}, Amount={Amount}",
                request.AccountNumber, request.Amount);

            return new OperationResponse(false, ex.Message);
        }
    }
}
