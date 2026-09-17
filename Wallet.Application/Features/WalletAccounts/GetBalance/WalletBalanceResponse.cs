using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Application.Features.WalletAccounts.GetBalance
{
    public sealed record WalletBalanceResponse(
    Guid WalletAccountId,
    decimal Balance,
    string Currency);
}
