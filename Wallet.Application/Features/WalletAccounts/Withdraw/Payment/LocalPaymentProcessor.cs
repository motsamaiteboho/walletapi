using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Application.Events;

namespace Wallet.Application.Features.WalletAccounts.Withdraw.Payment
{
    public class LocalPaymentProcessor : IPaymentProcessor
    {
        private readonly ILogger<LocalPaymentProcessor> _logger;

        public LocalPaymentProcessor(
            ILogger<LocalPaymentProcessor> logger)
        {
            _logger = logger;
        }

        public Task ProcessAsync(
            WalletWithdrawalEvent withdrawalEvent,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Payment processed successfully. WalletAccountId={WalletAccountId}, Amount={Amount}, Currency={Currency}",
                withdrawalEvent.WalletAccountId,
                withdrawalEvent.Amount,
                withdrawalEvent.Currency);

            return Task.CompletedTask;
        }
    }
}
