namespace TheFitzBankAPI.Application.Requests {
    public record DepositBody(decimal Amount);
    public record WithdrawBody(decimal Amount);
}
