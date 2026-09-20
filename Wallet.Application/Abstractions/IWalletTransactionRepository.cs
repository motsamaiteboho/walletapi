using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Domain.Entities;

namespace Wallet.Application.Abstractions
{
    public interface IWalletTransactionRepository
    {
        // Adds a wallet transaction to the repository for persistence.
        Task AddAsync(
            WalletTransaction transaction,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<WalletTransaction>> GetByWalletAccountIdAsync(
            Guid walletAccountId,
            CancellationToken cancellationToken = default);
    }
}
