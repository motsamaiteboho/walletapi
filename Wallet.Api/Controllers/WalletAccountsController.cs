using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wallet.Application.Abstractions;
using Wallet.Application.Features.WalletAccounts.GetTransactions;
using Wallet.Application.Features.WalletAccounts.Withdraw;

namespace Wallet.Api.Controllers
{
    [ApiController]
    [Route("api/wallet-accounts")]
    public class WalletAccountsController : ControllerBase
    {
        private readonly GetWalletBalanceService _balanceService;
        private readonly WithdrawWalletService _withdrawService;
        private readonly GetWalletTransactionsService _transactionsService;
        public WalletAccountsController(
            GetWalletBalanceService balanceService,
            WithdrawWalletService withdrawService,
            GetWalletTransactionsService transactionsService)
        {
            _balanceService = balanceService;
            _withdrawService = withdrawService;
            _transactionsService = transactionsService;
        }

        [HttpGet("{walletAccountId:guid}/balance")]
        public async Task<IActionResult> GetBalance(
            Guid walletAccountId,
            CancellationToken cancellationToken)
        {
            var result = await _balanceService.ExecuteAsync(
                walletAccountId,
                cancellationToken);

            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpPost("{walletAccountId:guid}/withdraw")]
        public async Task<IActionResult> Withdraw(Guid walletAccountId, [FromBody] WithdrawWalletRequest request,
            [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                return BadRequest(
                    "Idempotency-Key header is required.");
            }

            var result = await _withdrawService.ExecuteAsync(
                walletAccountId,
                request,
                idempotencyKey,
                cancellationToken);

            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpGet("{walletAccountId:guid}/transactions")]
        public async Task<IActionResult> GetTransactions(Guid walletAccountId, CancellationToken cancellationToken)
        {
            var result =
                await _transactionsService.ExecuteAsync(
                    walletAccountId,
                    cancellationToken);

            return Ok(result);
        }
    }
}
