using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wallet.Application.Abstractions;
using Wallet.Application.Features.WalletAccounts.GetTransactions;
using Wallet.Application.Features.WalletAccounts.Withdraw;

namespace Wallet.Api.Controllers
{
    [ApiController]
    [Route("api/wallet-accounts")]
    /// <summary>
    /// Controller for wallet account operations such as retrieving balance,
    /// withdrawing funds and listing transactions.
    /// </summary>
    /// <remarks>
    /// Endpoints in this controller operate on a specific wallet account
    /// identified by a GUID (walletAccountId).
    /// </remarks>
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

        /// <summary>
        /// Gets the current balance for the specified wallet account.
        /// </summary>
        /// <param name="walletAccountId">The unique identifier of the wallet account. Example: 11111111-1111-1111-1111-111111111111</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>200 OK with the balance when found, 404 NotFound when the account does not exist.</returns>
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

        /// <summary>
        /// Withdraws an amount from the specified wallet account.
        /// </summary>
        /// <param name="walletAccountId">The unique identifier of the wallet account. Example: 11111111-1111-1111-1111-111111111111</param>
        /// <param name="request">The withdraw request payload containing the amount and other details.</param>
        /// <param name="idempotencyKey">A unique idempotency key passed in the request header to prevent duplicate operations. Example: abc123-idem-0001</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>
        /// 200 OK with the withdraw result when successful,
        /// 400 BadRequest when required headers or payload are missing,
        /// 404 NotFound when the wallet account does not exist.
        /// </returns>
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

        /// <summary>
        /// Retrieves transactions for the specified wallet account.
        /// </summary>
        /// <param name="walletAccountId">The unique identifier of the wallet account. Example: 11111111-1111-1111-1111-111111111111</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>200 OK with a list of transactions.</returns>
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
