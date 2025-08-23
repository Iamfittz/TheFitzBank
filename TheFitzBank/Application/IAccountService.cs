namespace TheFitzBankAPI.Application {
    public interface IAccountService {
        Task<OperationResponse> CreateAccountAsync(CreateAccountRequest request);
        Task<AccountResponse?> GetAccountAsync(string accountNumber);
        Task<IReadOnlyList<AccountResponse>> GetAllAccountsAsync();
        Task<OperationResponse> DepositAsync(DepositRequest request);
        Task<TransferResponse> TransferAsync(TransferRequest request);
        Task<OperationResponse> WithdrawAsync(WithdrawRequest request);
    }


}