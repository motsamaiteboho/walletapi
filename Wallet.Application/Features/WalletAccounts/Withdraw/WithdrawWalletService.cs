using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Application.Abstractions;

namespace Wallet.Application.Features.WalletAccounts.Withdraw
{
    public class WithdrawWalletService
    {
        private readonly IWalletAccountRepository _repository;

        public WithdrawWalletService(
            IWalletAccountRepository repository)
        {
            _repository = repository;
        }

        public async Task<WithdrawWalletResponse?> ExecuteAsync(
            Guid walletAccountId,
            WithdrawWalletRequest request,
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

            walletAccount.Withdraw(request.Amount);

            await _repository.SaveChangesAsync(
                cancellationToken);

            return new WithdrawWalletResponse(
                walletAccount.Id,
                request.Amount,
                walletAccount.Balance,
                walletAccount.Currency);
        }
    }
}
