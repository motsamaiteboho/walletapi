using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Application.Events;

namespace Wallet.Application.Features.WalletAccounts.Withdraw
{
    public class LocalWithdrawalProcessor : IWithdrawalProcessor
    {
        private readonly ILogger<LocalWithdrawalProcessor> _logger;

        public LocalWithdrawalProcessor(
            ILogger<LocalWithdrawalProcessor> logger)
        {
            _logger = logger;
        }

        public Task ProcessAsync(
            WalletWithdrawalEvent withdrawalEvent,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Processing withdrawal downstream. WalletAccountId={WalletAccountId}, Amount={Amount}, Currency={Currency}",
                withdrawalEvent.WalletAccountId,
                withdrawalEvent.Amount,
                withdrawalEvent.Currency);

            return Task.CompletedTask;
        }
    }
}
