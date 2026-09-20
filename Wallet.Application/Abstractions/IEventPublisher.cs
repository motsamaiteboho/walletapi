using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Application.Abstractions
{
    public interface IEventPublisher
    {
        /// <summary>
        /// Publishes a domain event asynchronously to configured event sinks.
        /// </summary>
        Task PublishAsync<T>(
            T @event,
            CancellationToken cancellationToken = default);
    }
}
