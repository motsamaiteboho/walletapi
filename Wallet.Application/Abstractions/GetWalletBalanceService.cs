using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Application.Features.WalletAccounts.GetBalance;

namespace Wallet.Application.Abstractions
{
    public class GetWalletBalanceService
    {
        private readonly IWalletAccountRepository _repository;

        public GetWalletBalanceService(
            IWalletAccountRepository repository)
        {
            _repository = repository;
        }

        public async Task<WalletBalanceResponse?> ExecuteAsync(
            Guid walletAccountId,
            CancellationToken cancellationToken = default)
        {
            var walletAccount =
                await _repository.GetByIdAsync(
                    walletAccountId,
                    cancellationToken);

            if (walletAccount is null)
            {
                return null;
            }

            return new WalletBalanceResponse(
                walletAccount.Id,
                walletAccount.Balance,
                walletAccount.Currency);
        }
    }
}
