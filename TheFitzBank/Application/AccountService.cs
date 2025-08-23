using AutoMapper;
using TheFitzBankAPI.Application.Validators;
using TheFitzBankAPI.Domain;

namespace TheFitzBankAPI.Application.Services;

public sealed class AccountService : IAccountService {
    private readonly IAccountRepository _accountRepository;
    private readonly IMapper _mapper;

    public AccountService(IAccountRepository accountRepository, IMapper mapper) {
        _accountRepository = accountRepository;
        _mapper = mapper;
    }

    public async Task<OperationResponse> CreateAccountAsync(CreateAccountRequest request) {
        var validator = new CreateAccountRequestValidator();
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid) {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            return new OperationResponse(false, $"Validation failed: {errors}");
        }

        // Генерация номера счёта в формате ACC###### (6 цифр)
        string accNumber = "ACC" + Random.Shared.Next(100000, 999999);

        var account = new Account(
            accNumber,
            request.OwnerName,
            "USD",
            request.InitialBalance);

        await _accountRepository.AddAsync(account);

        var response = _mapper.Map<AccountResponse>(account);
        return new OperationResponse(true, "Account created successfully", response);
    }


    public async Task<AccountResponse?> GetAccountAsync(string accountNumber) {
        var account = await _accountRepository.GetByAccountNumberAsync(accountNumber);
        return account != null ? _mapper.Map<AccountResponse>(account) : null;
    }

    public async Task<IReadOnlyList<AccountResponse>> GetAllAccountsAsync() {
        var accounts = await _accountRepository.GetAllAsync();
        return _mapper.Map<IReadOnlyList<AccountResponse>>(accounts);
    }

    public async Task<OperationResponse> DepositAsync(DepositRequest request) {
        var validator = new DepositRequestValidator();
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid) {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            return new OperationResponse(false, $"Validation failed: {errors}");
        }

        var account = await _accountRepository.GetByAccountNumberAsync(request.AccountNumber);
        if (account == null)
            return new OperationResponse(false, "Account not found");

        account.Deposit(request.Amount);
        await _accountRepository.UpdateAsync(account);

        var response = _mapper.Map<AccountResponse>(account);
        return new OperationResponse(true, "Deposit successful", response);
    }

    public async Task<TransferResponse> TransferAsync(TransferRequest request) {
        var validator = new TransferRequestValidator();
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid) {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            return new TransferResponse(request.FromAccountNumber, request.ToAccountNumber,
                request.Amount, DateTime.UtcNow, false, $"Validation failed: {errors}");
        }

        var from = await _accountRepository.GetByAccountNumberAsync(request.FromAccountNumber);
        var to = await _accountRepository.GetByAccountNumberAsync(request.ToAccountNumber);

        if (from == null || to == null)
            return new TransferResponse(request.FromAccountNumber, request.ToAccountNumber,
                request.Amount, DateTime.UtcNow, false, "One or both accounts not found");

        try {
            from.TransferTo(to, request.Amount);
            await _accountRepository.UpdateAsync(from);
            await _accountRepository.UpdateAsync(to);

            return new TransferResponse(request.FromAccountNumber, request.ToAccountNumber,
                request.Amount, DateTime.UtcNow, true, "Transfer successful");
        } catch (Exception ex) {
            return new TransferResponse(request.FromAccountNumber, request.ToAccountNumber,
                request.Amount, DateTime.UtcNow, false, ex.Message);
        }
    }
    public async Task<OperationResponse> WithdrawAsync(WithdrawRequest request) {
        var account = await _accountRepository.GetByAccountNumberAsync(request.AccountNumber);
        if (account == null) {
            return new OperationResponse(false, "Account not found");
        }

        try {
            account.Withdraw(request.Amount);   // 💡 теперь тут вызывается твой метод
            await _accountRepository.UpdateAsync(account);

            return new OperationResponse(true, $"Withdrawn {request.Amount} from account {account.AccountNumber}");
        } catch (Exception ex) {
            return new OperationResponse(false, ex.Message);
        }
    }


}
