using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Application.Events;
using Wallet.Domain.Entities;

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

        public async Task ProcessAsync(
            WalletWithdrawalEvent withdrawalEvent,
            Guid eventId,
            CancellationToken cancellationToken = default)
        {
            var existingRecord =
                await _repository.GetByEventIdAsync(
                    eventId,
                    cancellationToken);

            if (existingRecord is not null)
            {
                _logger.LogInformation(
                    "Payment already processed. EventId={EventId}",
                    eventId);

                return;
            }

            var record = new PaymentProcessingRecord(
                eventId,
                withdrawalEvent.WalletAccountId,
                withdrawalEvent.Amount);

            await _repository.AddAsync(
                record,
                cancellationToken);

            // Simulate the downstream payment.
            _logger.LogInformation(
                "Processing payment. EventId={EventId}, WalletAccountId={WalletAccountId}, Amount={Amount}, Currency={Currency}",
                eventId,
                withdrawalEvent.WalletAccountId,
                withdrawalEvent.Amount,
                withdrawalEvent.Currency);

            record.MarkAsProcessed();

            await _repository.SaveChangesAsync(
                cancellationToken);

            _logger.LogInformation(
                "Payment processed successfully. EventId={EventId}",
                eventId);
        }
    }
}
