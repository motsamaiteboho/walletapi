using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Infrastructure.Persistence.Outbox
{
    public class OutboxMessage
    {
        public Guid Id { get; private set; }

        public string Type { get; private set; }

        public string Payload { get; private set; }

        public DateTime OccurredAt { get; private set; }

        public DateTime? ProcessedAt { get; private set; }

        public int RetryCount { get; private set; }

        public string? Error { get; private set; }

        private OutboxMessage()
        {
            // Required by EF Core
        }

        public OutboxMessage(
            Guid id,
            string type,
            string payload,
            DateTime occurredAt)
        {
            Id = id;
            Type = type;
            Payload = payload;
            OccurredAt = occurredAt;
            RetryCount = 0;
        }

        public void MarkAsProcessed()
        {
            ProcessedAt = DateTime.UtcNow;
            Error = null;
        }

        public void MarkAsFailed(string error)
        {
            RetryCount++;
            Error = error;
        }
    }
}
