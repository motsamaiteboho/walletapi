using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Application.Events;
using Wallet.Application.Features.WalletAccounts.Withdraw.Payment;

namespace Wallet.Application.Features.WalletAccounts.Withdraw
{
    public class LocalWithdrawalProcessor : IWithdrawalProcessor
    {
        private readonly IPaymentProcessor _paymentProcessor;

        public LocalWithdrawalProcessor(
            IPaymentProcessor paymentProcessor)
        {
            _paymentProcessor = paymentProcessor;
        }

        public Task ProcessAsync(
            WalletWithdrawalEvent withdrawalEvent,
            Guid eventId,
            CancellationToken cancellationToken = default)
        {
            return _paymentProcessor.ProcessAsync(
                withdrawalEvent,
                eventId,
                cancellationToken);
        }
    }
}
