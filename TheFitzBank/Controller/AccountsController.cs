using Microsoft.AspNetCore.Mvc;
using TheFitzBankAPI.Application;
using TheFitzBankAPI.Application.Requests;

namespace TheFitzBankAPI.Controller {
    [ApiController]
    [Route("api/accounts")]
    public class AccountsController : ControllerBase {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService) {
            this._accountService = accountService;
        }
        // POST /api/accounts
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAccountRequest request) {
            var result = await _accountService.CreateAccountAsync(request);
            if (!result.Success) return BadRequest(result);

            var created = (AccountResponse)result.Data!;
            return CreatedAtAction(nameof(Get), new { accountNumber = created.AccountNumber }, created);
        }
        // GET /api/accounts/{accountNumber}
        [HttpGet("{accountNumber}")]
        public async Task<IActionResult> Get(string accountNumber) {
            var account = await _accountService.GetAccountAsync(accountNumber);
            return account is not null ? Ok(account) : NotFound("Account not found");
        }
        // GET /api/accounts
        [HttpGet]
        public async Task<IActionResult> GetAll() {
            var accounts = await _accountService.GetAllAccountsAsync();
            return Ok(accounts);
        }

        // POST /api/accounts/{accountNumber}/deposits
        [HttpPost("{accountNumber}/deposits")]
        public async Task<IActionResult> Deposit(string accountNumber, [FromBody] DepositBody body) {
            var result = await _accountService.DepositAsync(new DepositRequest(accountNumber, body.Amount));
            return result.Success ? Ok(result) : BadRequest(result);
        }
        // POST /api/transfers
        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferRequest request) {
            var result = await _accountService.TransferAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        // POST /api/accounts/{accountNumber}/withdrawals
        [HttpPost("{accountNumber}/withdrawals")]
        public async Task<IActionResult> Withdraw(string accountNumber, [FromBody] WithdrawBody body) {
            var result = await _accountService.WithdrawAsync(new WithdrawRequest(accountNumber, body.Amount));
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
