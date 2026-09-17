using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Application.Events
{
    public sealed record WalletWithdrawalEvent(
     Guid WalletAccountId,
     decimal Amount,
     decimal RemainingBalance,
     string Currency,
     DateTime OccurredAt);
}
