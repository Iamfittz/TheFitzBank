using Microsoft.AspNetCore.Mvc;
using TheFitzBankAPI.Application;

namespace TheFitzBankAPI.Controller {
    [ApiController]
    [Route("api/transactions")]
    public class TransactionController : ControllerBase {
        private readonly IAccountService _accountService;

        public TransactionController(IAccountService accountService) {
            this._accountService = accountService;
        }

        // POST /api/transactions/deposit
        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositRequest body) {
            var result = await _accountService.DepositAsync(body);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        // POST /api/transactions/transfer
        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferRequest request) {
            var result = await _accountService.TransferAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        // POST /api/transactions/withdrawal
        [HttpPost("withdrawal")]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest body) {
            var result = await _accountService.WithdrawAsync(body);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
