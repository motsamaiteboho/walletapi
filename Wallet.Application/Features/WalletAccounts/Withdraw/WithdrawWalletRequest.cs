using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Application.Features.WalletAccounts.Withdraw
{
    public sealed record WithdrawWalletRequest(
    decimal Amount);
}
