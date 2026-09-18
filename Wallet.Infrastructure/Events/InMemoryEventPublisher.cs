using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Application.Abstractions;

namespace Wallet.Infrastructure.Events
{
    public class InMemoryEventPublisher : IEventPublisher
    {
        public Task PublishAsync<T>(
            T @event,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
