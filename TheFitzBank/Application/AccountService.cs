using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TheFitzBankAPI.Domain;
using TheFitzBankAPI.Infrastructure;

namespace TheFitzBankAPI.Application.Services;

public sealed class AccountService : IAccountService {
    private readonly IMapper _mapper;
    private readonly ILogger<AccountService> _logger;
    private readonly BankingContext _db;
    private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    public AccountService(IMapper mapper, ILogger<AccountService> logger, BankingContext db) {
        _mapper = mapper;
        _logger = logger;
        _db = db;
    }

    public async Task<Result<AccountResponse>> CreateAccountAsync(CreateAccountRequest request) {
        _logger.LogInformation("CreateAccount requested: Owner={Owner}, InitialBalance={Balance}",
            request.OwnerName, request.InitialBalance);

        await _semaphoreSlim.WaitAsync();

        try {
            using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            string accNumber;
            int attempts = 0;
            const int maxAttempts = 10;

            do {
                accNumber = "ACC" + Random.Shared.Next(100000, 999999);
                attempts++;

                if (attempts > maxAttempts) {
                    _logger.LogError("Failed to generate unique account number after {Attempts} attempts", maxAttempts);
                    return Result<AccountResponse>.Failure("Unable to generate unique account number", 500);
                }
            }
            while (await _db.Accounts.AnyAsync(a => a.AccountNumber == accNumber));

            var account = new Account(accNumber, request.OwnerName, "USD", request.InitialBalance);

            await _db.Accounts.AddAsync(account);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Account created: AccountNumber={AccountNumber}, Owner={Owner}, Balance={Balance}",
                account.AccountNumber, account.OwnerName, account.Balance);

            AccountResponse response = _mapper.Map<AccountResponse>(account);
            return Result<AccountResponse>.Success(response);
        } catch (DbUpdateException ex) {
            _logger.LogError(ex, "Create account failed with database exception: Owner={Owner}", request.OwnerName);
            return Result<AccountResponse>.Failure("Database error occurred", 500);
        } catch (Exception ex) {
            _logger.LogError(ex, "Create account failed with exception: Owner={Owner}", request.OwnerName);
            return Result<AccountResponse>.Failure("Server error", 500);
        } finally {
            _semaphoreSlim.Release();
        }
    }

    public async Task<Result<AccountResponse>> GetAccountAsync(string accountNumber) {
        _logger.LogInformation("GetAccount requested: AccountNumber={AccountNumber}", accountNumber);

        var account = await _db.Accounts
            .AsNoTracking() 
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

        if (account == null) {
            _logger.LogWarning("Account not found: AccountNumber={AccountNumber}", accountNumber);
            return Result<AccountResponse>.Failure("Account not found", 404);
        }

        _logger.LogInformation("Account found: AccountNumber={AccountNumber}, Owner={Owner}, Balance={Balance}",
            account.AccountNumber, account.OwnerName, account.Balance);

        return Result<AccountResponse>.Success(_mapper.Map<AccountResponse>(account));
    }
    public async Task<Result<IReadOnlyList<AccountResponse>>> GetAllAccountsAsync() {
        _logger.LogInformation("GetAllAccounts requested");

        var accounts = await _db.Accounts
            .AsNoTracking()
            .ToListAsync();

        _logger.LogInformation("Accounts retrieved: Count={Count}", accounts.Count);

        return Result<IReadOnlyList<AccountResponse>>.Success(_mapper.Map<IReadOnlyList<AccountResponse>>(accounts));
    }
    public async Task<Result<AccountResponse>> DepositAsync(DepositRequest request) {
        _logger.LogInformation("Deposit requested: Account={AccountNumber}, Amount={Amount}",
            request.AccountNumber, request.Amount);

        using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        try {
            
            var account = await _db.Accounts
                .Where(a => a.AccountNumber == request.AccountNumber)
                .FirstOrDefaultAsync();

            if (account == null) {
                _logger.LogWarning("Deposit failed: account not found: {AccountNumber}", request.AccountNumber);
                return Result<AccountResponse>.Failure("Account not found", 404);
            }

            account.Deposit(request.Amount);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Deposit successful: Account={AccountNumber}, NewBalance={Balance}",
                account.AccountNumber, account.Balance);

            var response = _mapper.Map<AccountResponse>(account);
            return Result<AccountResponse>.Success(response);
        } catch (DbUpdateConcurrencyException ex) {
            _logger.LogError(ex, "Deposit failed due to concurrency conflict: Account={AccountNumber}, Amount={Amount}",
                request.AccountNumber, request.Amount);
            return Result<AccountResponse>.Failure("Concurrency conflict occurred. Please try again.", 409);
        } catch (Exception ex) {
            _logger.LogError(ex, "Deposit failed with exception: Account={AccountNumber}, Amount={Amount}",
                request.AccountNumber, request.Amount);
            return Result<AccountResponse>.Failure("Server error", 500);
        }
    }

    public async Task<Result<TransferResponse>> TransferAsync(TransferRequest request) {
        _logger.LogInformation("Transfer requested: From={From}, To={To}, Amount={Amount}",
            request.FromAccountNumber, request.ToAccountNumber, request.Amount);

        using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        try {
            var accountNumbers = new[] { request.FromAccountNumber, request.ToAccountNumber }
                .OrderBy(x => x)
                .ToArray();

            var accounts = await _db.Accounts
                .Where(a => accountNumbers.Contains(a.AccountNumber))
                .OrderBy(a => a.AccountNumber) 
                .ToListAsync();

            var from = accounts.FirstOrDefault(a => a.AccountNumber == request.FromAccountNumber);
            var to = accounts.FirstOrDefault(a => a.AccountNumber == request.ToAccountNumber);

            if (from == null || to == null) {
                _logger.LogWarning("Transfer failed: One or both accounts not found. From={From}, To={To}",
                    request.FromAccountNumber, request.ToAccountNumber);

                return Result<TransferResponse>.Success(new TransferResponse(
                    request.FromAccountNumber,
                    request.ToAccountNumber,
                    request.Amount,
                    DateTime.UtcNow,
                    false,
                    "One or both accounts not found"));
            }

            if (request.Amount > from.Balance) {
                _logger.LogWarning("Transfer failed: Insufficient funds. From={From}, Balance={Balance}, Amount={Amount}",
                    request.FromAccountNumber, from.Balance, request.Amount);

                return Result<TransferResponse>.Failure("Insufficient funds to transfer", 400);
            }

            from.TransferTo(to, request.Amount);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Transfer successful: From={From} (Balance: {FromBalance}), To={To} (Balance: {ToBalance}), Amount={Amount}",
                request.FromAccountNumber, from.Balance, request.ToAccountNumber, to.Balance, request.Amount);

            return Result<TransferResponse>.Success(new TransferResponse(
                request.FromAccountNumber,
                request.ToAccountNumber,
                request.Amount,
                DateTime.UtcNow,
                true,
                "Transfer successful"));
        } catch (DbUpdateConcurrencyException ex) {
            _logger.LogError(ex, "Transfer failed due to concurrency conflict: From={From}, To={To}, Amount={Amount}",
                request.FromAccountNumber, request.ToAccountNumber, request.Amount);

            return Result<TransferResponse>.Success(new TransferResponse(
                request.FromAccountNumber,
                request.ToAccountNumber,
                request.Amount,
                DateTime.UtcNow,
                false,
                "Concurrency conflict occurred. Please try again."));
        } catch (Exception ex) {
            _logger.LogError(ex, "Transfer failed with exception: From={From}, To={To}, Amount={Amount}",
                request.FromAccountNumber, request.ToAccountNumber, request.Amount);

            return Result<TransferResponse>.Success(new TransferResponse(
                request.FromAccountNumber,
                request.ToAccountNumber,
                request.Amount,
                DateTime.UtcNow,
                false,
                ex.Message));
        }
    }

    public async Task<Result> WithdrawAsync(WithdrawRequest request) {
        _logger.LogInformation("Withdraw requested: Account={AccountNumber}, Amount={Amount}",
            request.AccountNumber, request.Amount);

        using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        try {
            var account = await _db.Accounts
                .FirstOrDefaultAsync(a => a.AccountNumber == request.AccountNumber);

            if (account == null) {
                _logger.LogWarning("Withdraw failed: Account not found: {AccountNumber}", request.AccountNumber);
                return Result.Failure("Account not found", 404);
            }

            if (request.Amount > account.Balance) {
                _logger.LogWarning("Withdraw failed: Insufficient funds. Account={AccountNumber}, Balance={Balance}, Amount={Amount}",
                    request.AccountNumber, account.Balance, request.Amount);

                return Result.Failure("Withdraw failed: Insufficient funds", 400);
            }

            account.Withdraw(request.Amount);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Withdraw successful: Account={AccountNumber}, NewBalance={Balance}",
                account.AccountNumber, account.Balance);

            return Result.Success();
        } catch (DbUpdateConcurrencyException ex) {
            _logger.LogError(ex, "Withdraw failed due to concurrency conflict: Account={AccountNumber}, Amount={Amount}",
                request.AccountNumber, request.Amount);
            return Result.Failure("Concurrency conflict occurred. Please try again.", 409);
        } catch (Exception ex) {
            _logger.LogError(ex, "Withdraw failed with exception: Account={AccountNumber}, Amount={Amount}",
                request.AccountNumber, request.Amount);
            return Result.Failure("Server error", 500);
        }
    }
}