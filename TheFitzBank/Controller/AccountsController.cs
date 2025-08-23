using Microsoft.AspNetCore.Mvc;
using TheFitzBankAPI.Application;

namespace TheFitzBankAPI.Controller {
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService) {
            this._accountService = accountService;
        }

        // POST: api/accounts/create
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateAccountRequest request) {
            var result = await _accountService.CreateAccountAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // GET: api/accounts/{accountNumber}
        [HttpGet("{accountNumber}")]
        public async Task<IActionResult> Get(string accountNumber) {
            var account = await _accountService.GetAccountAsync(accountNumber);
            return account != null ? Ok(account) : NotFound("Account not found");
        }

        // GET: api/accounts
        [HttpGet]
        public async Task<IActionResult> GetAll() {
            var accounts = await _accountService.GetAllAccountsAsync();
            return Ok(accounts);
        }

        // POST: api/accounts/deposit
        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositRequest request) {
            var result = await _accountService.DepositAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // POST: api/accounts/transfer
        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferRequest request) {
            var result = await _accountService.TransferAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest request) {
            var result = await _accountService.WithdrawAsync(request);
            if (!result.Success) {
                return BadRequest(result);
            }
            return Ok(result);
        }

    }
}
