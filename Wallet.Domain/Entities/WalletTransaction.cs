using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Domain.Enums;

namespace Wallet.Domain.Entities
{
    public class WalletTransaction
    {
        public Guid Id { get; private set; }

        public Guid WalletAccountId { get; private set; }

        public WalletTransactionType Type { get; private set; }

        public decimal Amount { get; private set; }

        public decimal BalanceAfter { get; private set; }

        public DateTime CreatedAt { get; private set; }

        private WalletTransaction()
        {
            // Required by EF Core
        }

        public WalletTransaction(
            Guid id,
            Guid walletAccountId,
            WalletTransactionType type,
            decimal amount,
            decimal balanceAfter)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(
                    "Transaction ID cannot be empty.",
                    nameof(id));
            }

            if (walletAccountId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Wallet account ID cannot be empty.",
                    nameof(walletAccountId));
            }

            if (amount <= 0)
            {
                throw new ArgumentException(
                    "Transaction amount must be greater than zero.",
                    nameof(amount));
            }

            if (balanceAfter < 0)
            {
                throw new ArgumentException(
                    "Balance after transaction cannot be negative.",
                    nameof(balanceAfter));
            }

            Id = id;
            WalletAccountId = walletAccountId;
            Type = type;
            Amount = amount;
            BalanceAfter = balanceAfter;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
