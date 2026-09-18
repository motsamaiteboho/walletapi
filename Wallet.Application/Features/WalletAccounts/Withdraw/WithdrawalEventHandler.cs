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
    public class WithdrawalEventHandler
    {
        private readonly ProcessWithdrawalEventService _processor;
        private readonly ILogger<WithdrawalEventHandler> _logger;

        public WithdrawalEventHandler(
            ProcessWithdrawalEventService processor,
            ILogger<WithdrawalEventHandler> logger)
        {
            _processor = processor;
            _logger = logger;
        }

        public async Task HandleAsync(
            WalletWithdrawalEvent withdrawalEvent,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _processor.ProcessAsync(
                    withdrawalEvent,
                    Guid.NewGuid(),
                    cancellationToken);
            }
            catch (PaymentValidationException exception)
            {
                _logger.LogError(
                    exception,
                    "Permanent payment validation failure.");

                throw;
            }
            catch (PaymentProcessingException exception)
            {
                _logger.LogWarning(
                    exception,
                    "Transient payment processing failure.");

                throw;
            }
        }
    }
}
