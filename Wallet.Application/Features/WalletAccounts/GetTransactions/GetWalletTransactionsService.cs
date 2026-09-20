using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Application.Abstractions;

namespace Wallet.Application.Features.WalletAccounts.GetTransactions
{
    public class GetWalletTransactionsService
    {
        private readonly IWalletTransactionRepository _repository;

        public GetWalletTransactionsService(
            IWalletTransactionRepository repository)
        {
            _repository = repository;
        }

        // Retrieves transactions for a wallet account and maps them to response DTOs.
        public async Task<IReadOnlyList<WalletTransactionResponse>>
            ExecuteAsync(
                Guid walletAccountId,
                CancellationToken cancellationToken = default)
        {
            var transactions =
                await _repository.GetByWalletAccountIdAsync(
                    walletAccountId,
                    cancellationToken);

            return transactions
                .Select(transaction =>
                    new WalletTransactionResponse(
                        transaction.Id,
                        transaction.WalletAccountId,
                        transaction.Type,
                        transaction.Amount,
                        transaction.BalanceAfter,
                        transaction.CreatedAt))
                .ToList();
        }
    }
}
