using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Domain.Exceptions;

namespace Wallet.Domain.Entities
{
    public class WalletAccount
    {
        public Guid Id { get; private set; }

        public decimal Balance { get; private set; }

        public string Currency { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public uint Version { get; private set; }

        // Parameterless ctor required by EF Core for materialization.
        private WalletAccount()
        {
            // Required by EF Core
        }

        // Creates a new wallet account ensuring valid initial state.
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

        // Withdraws an amount from the account. Validates amount and throws
        // InsufficientFundsException when balance is insufficient.
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
                throw new InsufficientFundsException();
            }

            Balance -= amount;
        }
    }
}
