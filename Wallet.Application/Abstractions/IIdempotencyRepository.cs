using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Application.Abstractions
{
    public interface IIdempotencyRepository
    {
        Task<IdempotencyRecord?> GetAsync(
            Guid walletAccountId,
            string idempotencyKey,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            IdempotencyRecord record,
            CancellationToken cancellationToken = default);
    }
}
