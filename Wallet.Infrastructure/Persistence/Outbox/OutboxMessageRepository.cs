using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Infrastructure.Persistence.Outbox
{
    public class OutboxMessageRepository
    : IOutboxMessageRepository
    {
        private readonly WalletDbContext _context;

        public OutboxMessageRepository(
            WalletDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(
            int batchSize,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            return await _context.OutboxMessages
                .Where(message =>
                    message.ProcessedAt == null &&
                    (message.NextRetryAt == null ||
                     message.NextRetryAt <= now))
                .OrderBy(message => message.OccurredAt)
                .Take(batchSize)
                .ToListAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(
                cancellationToken);
        }
    }
}
