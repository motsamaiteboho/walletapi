using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Application.Abstractions
{
    public interface IUnitOfWork
    {
        Task SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
