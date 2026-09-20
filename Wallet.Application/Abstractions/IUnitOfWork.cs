using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Application.Abstractions
{
    public interface IUnitOfWork
    {
        // Persists pending changes to the underlying store in a transactional
        // manner where supported.
        Task SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
