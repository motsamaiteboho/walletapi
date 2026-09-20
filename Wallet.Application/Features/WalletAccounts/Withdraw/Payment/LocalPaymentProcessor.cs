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

        // Processes the withdrawal payment locally: creates a processing record,
        // simulates downstream processing and marks the record as processed.
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
                    "Payment processing record already exists. EventId={EventId}",
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

            await _repository.SaveChangesAsync(
                cancellationToken);

            _logger.LogInformation(
                "Payment processing claimed. EventId={EventId}",
                eventId);

            // Local simulation of the downstream payment.
            _logger.LogInformation(
                "Processing payment. EventId={EventId}, Amount={Amount}, Currency={Currency}",
                eventId,
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
