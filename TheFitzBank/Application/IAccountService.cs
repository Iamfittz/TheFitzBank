namespace TheFitzBankAPI.Application {
    public interface IAccountService {
        Task<Result<AccountResponse>> CreateAccountAsync(CreateAccountRequest request);
        Task<Result<AccountResponse>> GetAccountAsync(string accountNumber);
        Task<Result<IReadOnlyList<AccountResponse>>> GetAllAccountsAsync();
        Task<Result<AccountResponse>> DepositAsync(DepositRequest request);
        Task<Result<TransferResponse>> TransferAsync(TransferRequest request);
        Task<Result> WithdrawAsync(WithdrawRequest request);
    }


}