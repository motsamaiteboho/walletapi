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
        Task AddAsync(
            WalletTransaction transaction,
            CancellationToken cancellationToken = default);
    }
}
