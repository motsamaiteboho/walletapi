using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Domain.Entities
{
    public class WalletAccount
    {
        public Guid Id { get; private set; }

        public decimal Balance { get; private set; }

        public string Currency { get; private set; }

        public DateTime CreatedAt { get; private set; }

        private WalletAccount()
        {
            // Required by EF Core
        }

        public WalletAccount(
            Guid id,
            decimal initialBalance,
            string currency)
        {
            if (id == Guid.Empty)
                throw new ArgumentException(
                    "Wallet ID cannot be empty.",
                    nameof(id));

            if (initialBalance < 0)
                throw new ArgumentException(
                    "Initial balance cannot be negative.",
                    nameof(initialBalance));

            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException(
                    "Currency is required.",
                    nameof(currency));

            Id = id;
            Balance = initialBalance;
            Currency = currency;
            CreatedAt = DateTime.UtcNow;
        }

        public void Withdraw(decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException(
                    "Withdrawal amount must be greater than zero.",
                    nameof(amount));
            }

            if (amount > Balance)
            {
                throw new InvalidOperationException(
                    "Insufficient funds.");
            }

            Balance -= amount;
        }
    }
}
