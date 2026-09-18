using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Infrastructure.Persistence
{
    public class IdempotencyRecordEntity
    {
        public Guid Id { get; private set; }

        public Guid WalletAccountId { get; private set; }

        public string IdempotencyKey { get; private set; }

        public string RequestHash { get; private set; }

        public string ResponsePayload { get; private set; }

        public int StatusCode { get; private set; }

        public DateTime CreatedAt { get; private set; }

        private IdempotencyRecordEntity()
        {
        }

        public IdempotencyRecordEntity(
            Guid id,
            Guid walletAccountId,
            string idempotencyKey,
            string requestHash,
            string responsePayload,
            int statusCode,
            DateTime createdAt)
        {
            Id = id;
            WalletAccountId = walletAccountId;
            IdempotencyKey = idempotencyKey;
            RequestHash = requestHash;
            ResponsePayload = responsePayload;
            StatusCode = statusCode;
            CreatedAt = createdAt;
        }
    }
}
