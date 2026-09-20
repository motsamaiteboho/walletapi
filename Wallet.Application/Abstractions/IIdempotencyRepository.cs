using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Application.Abstractions
{
    public interface IIdempotencyRepository
    {
        // Retrieves an idempotency record for the specified wallet account and idempotency key.
        Task<IdempotencyRecord?> GetAsync(
            Guid walletAccountId,
            string idempotencyKey,
            CancellationToken cancellationToken = default);

        // Adds a new idempotency record for the idempotent operation.
        Task AddAsync(
            IdempotencyRecord record,
            CancellationToken cancellationToken = default);
    }
}
