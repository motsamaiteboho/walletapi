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
        Task<WalletAccount?> GetByIdAsync(
            Guid walletAccountId,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            WalletAccount walletAccount,
            CancellationToken cancellationToken = default);

        Task SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
