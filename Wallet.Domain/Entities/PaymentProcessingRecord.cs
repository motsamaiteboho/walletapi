using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Domain.Enums;

namespace Wallet.Domain.Entities
{
    public class PaymentProcessingRecord
    {
        public Guid Id { get; private set; }

        public Guid EventId { get; private set; }

        public Guid WalletAccountId { get; private set; }

        public decimal Amount { get; private set; }

        public PaymentProcessingStatus Status { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime? ProcessedAt { get; private set; }

        private PaymentProcessingRecord()
        {
        }

        public PaymentProcessingRecord(
     Guid eventId,
     Guid walletAccountId,
     decimal amount)
        {
            if (eventId == Guid.Empty)
                throw new ArgumentException(
                    "Event ID cannot be empty.",
                    nameof(eventId));

            if (walletAccountId == Guid.Empty)
                throw new ArgumentException(
                    "Wallet account ID cannot be empty.",
                    nameof(walletAccountId));

            if (amount <= 0)
                throw new ArgumentException(
                    "Payment amount must be greater than zero.",
                    nameof(amount));

            Id = Guid.NewGuid();
            EventId = eventId;
            WalletAccountId = walletAccountId;
            Amount = amount;
            Status = PaymentProcessingStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }

        public void MarkAsProcessed()
        {
            Status = PaymentProcessingStatus.Processed;
            ProcessedAt = DateTime.UtcNow;
        }
    }
}
