using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Application.Abstractions;

namespace Wallet.Infrastructure.Persistence
{
    public class WalletUnitOfWork : IUnitOfWork
    {
        private readonly WalletDbContext _context;

        public WalletUnitOfWork(
            WalletDbContext context)
        {
            _context = context;
        }

        public async Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(
                cancellationToken);
        }
    }
}
