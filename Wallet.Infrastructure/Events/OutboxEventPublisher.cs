using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Wallet.Application.Abstractions;
using Wallet.Infrastructure.Persistence;
using Wallet.Infrastructure.Persistence.Outbox;

namespace Wallet.Infrastructure.Events
{
    public class OutboxEventPublisher : IEventPublisher
    {
        private readonly WalletDbContext _context;

        public OutboxEventPublisher(
            WalletDbContext context)
        {
            _context = context;
        }

        // Stores the event payload in the outbox table for later reliable delivery.
        public async Task PublishAsync<T>(
            T @event,
            CancellationToken cancellationToken = default)
        {
            var outboxMessage = new OutboxMessage(
                Guid.NewGuid(),
                typeof(T).Name,
                JsonSerializer.Serialize(@event),
                DateTime.UtcNow);

            await _context.OutboxMessages.AddAsync(
                outboxMessage,
                cancellationToken);
        }
    }
}
