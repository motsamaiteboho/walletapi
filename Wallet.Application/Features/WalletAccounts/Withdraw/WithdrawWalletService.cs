using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Wallet.Application.Abstractions;
using Wallet.Application.Events;
using Wallet.Application.Exceptions;

namespace Wallet.Application.Features.WalletAccounts.Withdraw
{
    public class WithdrawWalletService
    {
        private readonly IWalletAccountRepository _repository;
        private readonly IEventPublisher _eventPublisher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<WithdrawWalletService> _logger;
        private readonly IIdempotencyRepository _idempotencyRepository;

        public WithdrawWalletService(
            IWalletAccountRepository repository,
            IEventPublisher eventPublisher,
            IUnitOfWork unitOfWork,
            IIdempotencyRepository idempotencyRepository,
            ILogger<WithdrawWalletService> logger)
        {
            _repository = repository;
            _eventPublisher = eventPublisher;
            _unitOfWork = unitOfWork;
            _idempotencyRepository = idempotencyRepository;
            _logger = logger;
        }

        public async Task<WithdrawWalletResponse?> ExecuteAsync(
     Guid walletAccountId,
     WithdrawWalletRequest request,
     string idempotencyKey,
     CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                throw new ArgumentException(
                    "Idempotency key is required.",
                    nameof(idempotencyKey));
            }

            var requestHash = CreateRequestHash(request);

            var existingRecord =
                await _idempotencyRepository.GetAsync(
                    walletAccountId,
                    idempotencyKey,
                    cancellationToken);

            if (existingRecord is not null)
            {
                if (existingRecord.RequestHash != requestHash)
                {
                    throw new IdempotencyKeyConflictException();
                }

                _logger.LogInformation(
                    "Returning existing idempotent withdrawal response. " +
                    "WalletAccountId={WalletAccountId}, IdempotencyKey={IdempotencyKey}",
                    walletAccountId,
                    idempotencyKey);

                return JsonSerializer.Deserialize<WithdrawWalletResponse>(
                    existingRecord.ResponsePayload);
            }

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

            _logger.LogInformation(
                "Processing wallet withdrawal. " +
                "WalletAccountId={WalletAccountId}, Amount={Amount}, Currency={Currency}",
                walletAccount.Id,
                request.Amount,
                walletAccount.Currency);

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

            var response = new WithdrawWalletResponse(
                walletAccount.Id,
                request.Amount,
                walletAccount.Balance,
                walletAccount.Currency);

            var idempotencyRecord = new IdempotencyRecord(
                walletAccount.Id,
                idempotencyKey,
                requestHash,
                JsonSerializer.Serialize(response),
                StatusCodes.Status200OK,
                DateTime.UtcNow);

            await _idempotencyRepository.AddAsync(idempotencyRecord, cancellationToken);

            try
            {
                await _unitOfWork.SaveChangesAsync(
                    cancellationToken);
            }
            catch (DuplicateIdempotencyKeyException)
            {
                existingRecord =
                    await _idempotencyRepository.GetAsync(
                        walletAccount.Id,
                        idempotencyKey,
                        cancellationToken);

                if (existingRecord is null)
                {
                    throw;
                }

                if (existingRecord.RequestHash != requestHash)
                {
                    throw new IdempotencyKeyConflictException();
                }

                _logger.LogInformation(
                    "Concurrent idempotent request detected. " +
                    "Returning existing withdrawal response. " +
                    "WalletAccountId={WalletAccountId}, IdempotencyKey={IdempotencyKey}",
                    walletAccount.Id,
                    idempotencyKey);

                return JsonSerializer.Deserialize<WithdrawWalletResponse>(
                    existingRecord.ResponsePayload);
            }

            return response;
        }

        private static string CreateRequestHash( WithdrawWalletRequest request)
        {
            var normalizedAmount =
                request.Amount.ToString(
                    "0.00",
                    CultureInfo.InvariantCulture);

            var bytes = SHA256.HashData(
                Encoding.UTF8.GetBytes(normalizedAmount));

            return Convert.ToHexString(bytes);
        }
    }
}
