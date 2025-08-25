using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using TheFitzBankAPI.Domain;
using TheFitzBankAPI.Infrastructure;

namespace TheFitzBankAPI.Application.Services;

public sealed class AccountService : IAccountService {

    private readonly IMapper _mapper;
    private readonly ILogger<AccountService> _logger;
    private readonly BankingContext _db;
    public AccountService(IMapper mapper, ILogger<AccountService> logger, BankingContext db) {
        _mapper = mapper;
        _logger = logger;
        _db = db;
    }

    public async Task<Result<AccountResponse>> CreateAccountAsync(CreateAccountRequest request) { 
        _logger.LogInformation("CreateAccount requested: Owner={Owner}, InitialBalance={Balance}", request.OwnerName, request.InitialBalance);
       
        string accNumber = "ACC" + Random.Shared.Next(100000, 999999);
        var account = new Account( accNumber,request.OwnerName,"USD",request.InitialBalance);

        await _db.Accounts.AddAsync(account);
        try {
            await _db.SaveChangesAsync();
        } catch (DbUpdateException ex) {
            _logger.LogError(ex, "Create account failed with exception: Owner={Owner}", request.OwnerName);
            return Result<AccountResponse>.Failure(ex.Message);

        } catch (Exception ex) {
            _logger.LogError(ex, "Create account failed with exception: Owner={Owner}",request.OwnerName);
            return Result<AccountResponse>.Failure("Server error", errorCode: 500);
        }

        _logger.LogInformation("Account created: AccountNumber={AccountNumber}, Owner={Owner}, Balance={Balance}",
            account.AccountNumber, account.OwnerName, account.Balance);

        AccountResponse response = _mapper.Map<AccountResponse>(account);
        return Result<AccountResponse>.Success(response);
    }
    public async Task<Result<AccountResponse>> GetAccountAsync(string accountNumber) {
            _logger.LogInformation("GetAccount requested: AccountNumber={AccountNumber}", accountNumber);

            var account = await _db.Accounts
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

            if (account == null) {
                _logger.LogWarning("Account not found: AccountNumber={AccountNumber}", accountNumber);
                return Result<AccountResponse>.Failure("Account not found");
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

        var account = await _db.Accounts
            .FirstOrDefaultAsync(a => a.AccountNumber == request.AccountNumber);
        if (account == null) {
            _logger.LogWarning("Deposit failed: account not found: {AccountNumber}", request.AccountNumber);
            return Result<AccountResponse>.Failure("Account not found");
        }

        account.Deposit(request.Amount);
        try {
            await _db.SaveChangesAsync();
        } catch (DbUpdateException ex) {
            _logger.LogError(ex, "Deposit failed with exception: Account={AccountNumber}, Amount={Amount}",
                request.AccountNumber, request.Amount);
            return Result<AccountResponse>.Failure(ex.Message);
        } catch (Exception ex) {
            _logger.LogError(ex, "Deposit failed with exception: Account={AccountNumber}, Amount={Amount}",
                request.AccountNumber, request.Amount);
            return Result<AccountResponse>.Failure("Server error", errorCode: 500);
        }
        _logger.LogInformation("Deposit successful: Account={AccountNumber}, NewBalance={Balance}",
            account.AccountNumber, account.Balance);

        var response = _mapper.Map<AccountResponse>(account);
        return Result<AccountResponse>.Success(response);
    }
    public async Task<Result<TransferResponse>> TransferAsync(TransferRequest request) {
        _logger.LogInformation("Transfer requested: From={From}, To={To}, Amount={Amount}",
            request.FromAccountNumber, request.ToAccountNumber, request.Amount);
        
        var from = await _db.Accounts
            .FirstOrDefaultAsync(a => a.AccountNumber == request.FromAccountNumber);
        var to = await _db.Accounts
            .FirstOrDefaultAsync(a => a.AccountNumber == request.ToAccountNumber);

        if (from == null || to == null) {
            _logger.LogWarning("Transfer failed: One or both accounts not found. From={From}, To={To}",
                request.FromAccountNumber, request.ToAccountNumber);

            return Result< TransferResponse>.Success(new TransferResponse(request.FromAccountNumber, request.ToAccountNumber,
                request.Amount, DateTime.UtcNow, false, "One or both accounts not found"));
        }

        try {
            from.TransferTo(to, request.Amount);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Transfer successful: From={From}, To={To}, Amount={Amount}",
                request.FromAccountNumber, request.ToAccountNumber, request.Amount);

            return Result<TransferResponse>.Success(new TransferResponse(request.FromAccountNumber, request.ToAccountNumber,
                request.Amount, DateTime.UtcNow, true, "Transfer successful"));
        } catch (Exception ex) {
            _logger.LogError(ex, "Transfer failed with exception: From={From}, To={To}, Amount={Amount}",
                request.FromAccountNumber, request.ToAccountNumber, request.Amount);

            return Result<TransferResponse>.Success(new TransferResponse(request.FromAccountNumber, request.ToAccountNumber,
                request.Amount, DateTime.UtcNow, false, ex.Message));
        }
    }
    public async Task<Result> WithdrawAsync(WithdrawRequest request) {
        _logger.LogInformation("Withdraw requested: Account={AccountNumber}, Amount={Amount}",
            request.AccountNumber, request.Amount);

        var account = await _db.Accounts
            .FirstOrDefaultAsync(a => a.AccountNumber == request.AccountNumber);
        if (account == null) {
            _logger.LogWarning("Withdraw failed: Account not found: {AccountNumber}", request.AccountNumber);
            return Result.Failure("Account not found");
        }

        try {
            account.Withdraw(request.Amount);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Withdraw successful: Account={AccountNumber}, NewBalance={Balance}",
                account.AccountNumber, account.Balance);

            return Result.Success();
        } catch (DbUpdateException ex) {
            _logger.LogError(ex, "Withdraw failed with exception: Account={AccountNumber}, Amount={Amount}",
                request.AccountNumber, request.Amount);
            return Result.Failure(ex.Message);
        } catch (Exception ex) {
            _logger.LogError(ex, "Withdraw failed with exception: Account={AccountNumber}, Amount={Amount}",
                request.AccountNumber, request.Amount);
            return Result.Failure("Server error",errorCode:500);
        }
    }

}
