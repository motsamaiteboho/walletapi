using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wallet.Application.Abstractions;
using Wallet.Application.Features.WalletAccounts.Withdraw;

namespace Wallet.Api.Controllers
{
    [ApiController]
    [Route("api/wallet-accounts")]
    public class WalletAccountsController : ControllerBase
    {
        private readonly GetWalletBalanceService _balanceService;
        private readonly WithdrawWalletService _withdrawService;

        public WalletAccountsController(
            GetWalletBalanceService balanceService,
            WithdrawWalletService withdrawService)
        {
            _balanceService = balanceService;
            _withdrawService = withdrawService;
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
        public async Task<IActionResult> Withdraw(
            Guid walletAccountId,
            [FromBody] WithdrawWalletRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _withdrawService.ExecuteAsync(
                walletAccountId,
                request,
                cancellationToken);

            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }
    }
}
