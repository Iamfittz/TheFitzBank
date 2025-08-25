using Microsoft.AspNetCore.Mvc;
using TheFitzBankAPI.Application;

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
    }
}
