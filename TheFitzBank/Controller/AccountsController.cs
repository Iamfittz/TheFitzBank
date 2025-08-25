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
            Result<AccountResponse> result = await _accountService.CreateAccountAsync(request);
            if (!result.IsSuccess) return BadRequest(result);

            var created = result.Data;
            return CreatedAtAction(nameof(Get), new { accountNumber = created.AccountNumber }, created);
        }
        // GET /api/accounts/{accountNumber}
        [HttpGet("{accountNumber}")]
        public async Task<IActionResult> Get(string accountNumber) {
            Result<AccountResponse> result = await _accountService.GetAccountAsync(accountNumber);

            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result.Data);
        }
        // GET /api/accounts
        [HttpGet]
        public async Task<IActionResult> GetAll() {
            Result<IReadOnlyList<AccountResponse>> result = await _accountService.GetAllAccountsAsync();
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result.Data);
        }
    }
}
