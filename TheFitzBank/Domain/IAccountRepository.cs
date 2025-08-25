namespace TheFitzBankAPI.Domain;

public interface IAccountRepository {
    Task<Account?> GetByIdAsync(int id);
    Task<Account?> GetByAccountNumberAsync(string accountNumber);
    Task<IReadOnlyList<Account>> GetAllAsync();

    Task AddAsync(Account account);
    Task UpdateAsync(Account account);
    Task<bool> ExistsAsync(string accountNumber);
}
