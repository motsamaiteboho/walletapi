using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Application.Events;

namespace Wallet.Application.Features.WalletAccounts.Withdraw
{
    public interface IWithdrawalProcessor
    {
        Task ProcessAsync(
            WalletWithdrawalEvent withdrawalEvent,
            CancellationToken cancellationToken = default);
    }
}
