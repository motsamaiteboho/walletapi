using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Application.Abstractions;

namespace Wallet.Infrastructure.Persistence.Idempotency
{
    public class IdempotencyRepository : IIdempotencyRepository
    {
        private readonly WalletDbContext _context;

        public IdempotencyRepository(
            WalletDbContext context)
        {
            _context = context;
        }

        // Retrieves an idempotency record entity and maps it to the domain record.
        public async Task<IdempotencyRecord?> GetAsync(
            Guid walletAccountId,
            string idempotencyKey,
            CancellationToken cancellationToken = default)
        {
            var entity = await _context.IdempotencyRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x =>
                        x.WalletAccountId == walletAccountId &&
                        x.IdempotencyKey == idempotencyKey,
                    cancellationToken);

            if (entity is null)
            {
                return null;
            }

            return new IdempotencyRecord(
                entity.WalletAccountId,
                entity.IdempotencyKey,
                entity.RequestHash,
                entity.ResponsePayload,
                entity.StatusCode,
                entity.CreatedAt);
        }

        // Persists a new idempotency record entity for later lookup.
        public async Task AddAsync(
            IdempotencyRecord record,
            CancellationToken cancellationToken = default)
        {
            var entity = new IdempotencyRecordEntity(
                Guid.NewGuid(),
                record.WalletAccountId,
                record.IdempotencyKey,
                record.RequestHash,
                record.ResponsePayload,
                record.StatusCode,
                record.CreatedAt);

            await _context.IdempotencyRecords.AddAsync(
                entity,
                cancellationToken);
        }
    }
}
