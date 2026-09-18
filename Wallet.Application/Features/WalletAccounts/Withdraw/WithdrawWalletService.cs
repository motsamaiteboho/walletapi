using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Application.Abstractions;
using Wallet.Application.Events;

namespace Wallet.Application.Features.WalletAccounts.Withdraw
{
    public class WithdrawWalletService
    {
        private readonly IWalletAccountRepository _repository;
        private readonly IEventPublisher _eventPublisher;
        private readonly IUnitOfWork _unitOfWork;
        public WithdrawWalletService(
            IWalletAccountRepository repository,
            IEventPublisher eventPublisher,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _eventPublisher = eventPublisher;
            _unitOfWork = unitOfWork;
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

            var withdrawalEvent = new WalletWithdrawalEvent(
                walletAccount.Id,
                request.Amount,
                walletAccount.Balance,
                walletAccount.Currency,
                DateTime.UtcNow);

            await _eventPublisher.PublishAsync(
                withdrawalEvent,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            return new WithdrawWalletResponse(
                walletAccount.Id,
                request.Amount,
                walletAccount.Balance,
                walletAccount.Currency);
        }
    }
}
