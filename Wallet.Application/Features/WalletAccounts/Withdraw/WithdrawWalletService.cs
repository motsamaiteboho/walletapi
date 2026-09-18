using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Application.Abstractions;
using Wallet.Application.Events;
using Microsoft.Extensions.Logging;

namespace Wallet.Application.Features.WalletAccounts.Withdraw
{
    public class WithdrawWalletService
    {
        private readonly IWalletAccountRepository _repository;
        private readonly IEventPublisher _eventPublisher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<WithdrawWalletService> _logger;

        public WithdrawWalletService(
            IWalletAccountRepository repository,
            IEventPublisher eventPublisher,
            IUnitOfWork unitOfWork,
            ILogger<WithdrawWalletService> logger)
        {
            _repository = repository;
            _eventPublisher = eventPublisher;
            _unitOfWork = unitOfWork;
            _logger = logger;
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
                _logger.LogWarning(
                    "Wallet account not found. WalletAccountId={WalletAccountId}",
                    walletAccountId);

                return null;
            }

            _logger.LogInformation("Processing wallet withdrawal. WalletAccountId={WalletAccountId}, Amount={Amount}, Currency={Currency}",
                walletAccount.Id,
                request.Amount,
                walletAccount.Currency);

            walletAccount.Withdraw(request.Amount);

            _logger.LogInformation("Wallet withdrawal applied. WalletAccountId={WalletAccountId}, Amount={Amount}, RemainingBalance={RemainingBalance}",
                walletAccount.Id,
                request.Amount,
                walletAccount.Balance);

            var withdrawalEvent = new WalletWithdrawalEvent(
                walletAccount.Id,
                request.Amount,
                walletAccount.Balance,
                walletAccount.Currency,
                DateTime.UtcNow);

            await _eventPublisher.PublishAsync(
                withdrawalEvent,
                cancellationToken);

            _logger.LogInformation("Wallet withdrawal event added to outbox. WalletAccountId={WalletAccountId}, Amount={Amount}",
                walletAccount.Id,
                request.Amount);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            _logger.LogInformation("Wallet withdrawal persisted. WalletAccountId={WalletAccountId}, Amount={Amount}",
                walletAccount.Id,
                request.Amount);

            return new WithdrawWalletResponse(
                walletAccount.Id,
                request.Amount,
                walletAccount.Balance,
                walletAccount.Currency);
        }
    }
}
