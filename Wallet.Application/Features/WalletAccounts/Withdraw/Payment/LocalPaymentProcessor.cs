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
        private readonly IPaymentProcessingRepository _repository;
        private readonly ILogger<LocalPaymentProcessor> _logger;

        public LocalPaymentProcessor(
            IPaymentProcessingRepository repository,
            ILogger<LocalPaymentProcessor> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task ProcessAsync(  WalletWithdrawalEvent withdrawalEvent,
            CancellationToken cancellationToken = default)
        {
            var existingRecord =
                await _repository.GetByEventIdAsync(
                    withdrawalEvent.WalletAccountId,
                    cancellationToken);
        }
    }
}
