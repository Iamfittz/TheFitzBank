using Microsoft.EntityFrameworkCore;
using TheFitzBankAPI.Domain;

namespace TheFitzBankAPI.Infrastructure;

public class AccountRepository : IAccountRepository {
    private readonly BankingContext _db;

    public AccountRepository(BankingContext db) {
        this._db = db;
    }

    public async Task<Account?> GetByIdAsync(int id) {
        return await _db.Accounts.FindAsync(id);
    }

    public async Task<Account?> GetByAccountNumberAsync(string accountNumber) {
        return await _db.Accounts
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
    }

    public async Task<IReadOnlyList<Account>> GetAllAsync() {
        return await _db.Accounts
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(Account account) {
        await _db.Accounts.AddAsync(account);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Account account) {
        _db.Accounts.Update(account);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(string accountNumber) {
        return await _db.Accounts
            .AnyAsync(a => a.AccountNumber == accountNumber);
    }
}
