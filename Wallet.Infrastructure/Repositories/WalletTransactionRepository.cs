using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Application.Abstractions;
using Wallet.Domain.Entities;
using Wallet.Infrastructure.Persistence;

namespace Wallet.Infrastructure.Repositories
{
    public class WalletTransactionRepository: IWalletTransactionRepository
    {
        private readonly WalletDbContext _context;

        public WalletTransactionRepository(
            WalletDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            WalletTransaction transaction,
            CancellationToken cancellationToken = default)
        {
            await _context.WalletTransactions.AddAsync(
                transaction,
                cancellationToken);
        }
    }
}
