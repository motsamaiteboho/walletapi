using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Domain.Entities;

namespace Wallet.Application.Abstractions
{
    public interface IWalletAccountRepository
    {
        // Fetches a wallet account by its identifier. Returns null when not found.
        Task<WalletAccount?> GetByIdAsync(
            Guid walletAccountId,
            CancellationToken cancellationToken = default);

        // Adds a new wallet account to the repository.
        Task AddAsync(
            WalletAccount walletAccount,
            CancellationToken cancellationToken = default);

    }
}
