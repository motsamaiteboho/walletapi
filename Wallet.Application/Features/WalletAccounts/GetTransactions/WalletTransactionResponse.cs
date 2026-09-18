using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Domain.Enums;

namespace Wallet.Application.Features.WalletAccounts.GetTransactions
{
    public sealed record WalletTransactionResponse(
        Guid TransactionId,
        Guid WalletAccountId,
        WalletTransactionType Type,
        decimal Amount,
        decimal BalanceAfter,
        DateTime CreatedAt);
}
