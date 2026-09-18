using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Application.Abstractions;
using Wallet.Application.Exceptions;
using Wallet.Domain.Entities;
using Wallet.Infrastructure.Persistence;

namespace Wallet.Infrastructure.Repositories
{
    public class WalletAccountRepository : IWalletAccountRepository
    {
        private readonly WalletDbContext _context;

        public WalletAccountRepository(
            WalletDbContext context)
        {
            _context = context;
        }

        public async Task<WalletAccount?> GetByIdAsync(
            Guid walletAccountId,
            CancellationToken cancellationToken = default)
        {
            return await _context.WalletAccounts
                .FirstOrDefaultAsync(
                    account => account.Id == walletAccountId,
                    cancellationToken);
        }

        public async Task AddAsync(
            WalletAccount walletAccount,
            CancellationToken cancellationToken = default)
        {
            await _context.WalletAccounts.AddAsync(
                walletAccount,
                cancellationToken);
        }
    }
}
