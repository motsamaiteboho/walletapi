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
using Wallet.Domain.Entities;
using Wallet.Domain.Enums;

namespace Wallet.Application.Features.WalletAccounts.Withdraw
{
    public class WithdrawWalletService
    {
        private readonly IWalletAccountRepository _repository;
        private readonly IEventPublisher _eventPublisher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<WithdrawWalletService> _logger;
        private readonly IIdempotencyRepository _idempotencyRepository;
        private readonly IWalletTransactionRepository _transactionRepository;
        public WithdrawWalletService(
            IWalletAccountRepository repository,
            IEventPublisher eventPublisher,
            IUnitOfWork unitOfWork,
            IIdempotencyRepository idempotencyRepository,
            IWalletTransactionRepository transactionRepository,
            ILogger<WithdrawWalletService> logger)
        {
            _repository = repository;
            _eventPublisher = eventPublisher;
            _unitOfWork = unitOfWork;
            _idempotencyRepository = idempotencyRepository;
            _transactionRepository = transactionRepository;
            _logger = logger;
        }

    // Executes a withdrawal for the specified wallet account.
    // Validates idempotency, applies domain logic, records a transaction,
    // publishes an event and saves changes atomically.
    public async Task<WithdrawWalletResponse?> ExecuteAsync(
        Guid walletAccountId,
        WithdrawWalletRequest request,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
        {
            // Ensure an idempotency key was provided to allow safe retries.
            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                throw new ArgumentException(
                    "Idempotency key is required.",
                    nameof(idempotencyKey));
            }

            // Compute a deterministic hash of the request so we can detect
            // reuse of the same idempotency key with a different payload.
            var requestHash = CreateRequestHash(request);

            // Look up an existing idempotency record for this wallet and key.
            var existingRecord =
                await _idempotencyRepository.GetAsync(
                    walletAccountId,
                    idempotencyKey,
                    cancellationToken);

            // If an existing record is present, validate the hash and return
            // the stored response to enforce idempotent behavior.
            if (existingRecord is not null)
            {
                if (existingRecord.RequestHash != requestHash)
                {
                    // Same key but different payload — treat as a conflict.
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

            // Load the wallet account from the repository.
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

            // Log the start of the withdrawal processing.
            _logger.LogInformation(
                "Processing wallet withdrawal. " +
                "WalletAccountId={WalletAccountId}, Amount={Amount}, Currency={Currency}",
                walletAccount.Id,
                request.Amount,
                walletAccount.Currency);

            // Apply domain logic for the withdrawal. This may throw domain exceptions
            // such as insufficient funds, which should propagate to the caller.
            walletAccount.Withdraw(request.Amount);

            // Log state after applying the withdrawal.
            _logger.LogInformation( "Wallet withdrawal applied. WalletAccountId={WalletAccountId}, Amount={Amount}, RemainingBalance={RemainingBalance}",
                walletAccount.Id,
                request.Amount,
                walletAccount.Balance);

            // Create a transaction entity representing this withdrawal.
            var transaction = new WalletTransaction(
                Guid.NewGuid(),
                walletAccount.Id,
                WalletTransactionType.Withdrawal,
                request.Amount,
                walletAccount.Balance);

            await _transactionRepository.AddAsync(
                transaction,
                cancellationToken);

            // Publish a domain event to inform other systems of the withdrawal.
            var withdrawalEvent = new WalletWithdrawalEvent(
                walletAccount.Id,
                request.Amount,
                walletAccount.Balance,
                walletAccount.Currency,
                DateTime.UtcNow);

            await _eventPublisher.PublishAsync(
                withdrawalEvent,
                cancellationToken);

            // Prepare the response payload for the caller.
            var response = new WithdrawWalletResponse(
                walletAccount.Id,
                request.Amount,
                walletAccount.Balance,
                walletAccount.Currency);

            // Create and persist an idempotency record capturing the request/response.
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
                // Persist all changes atomically via the unit of work. A concurrent
                // request may cause a DuplicateIdempotencyKeyException if it inserted
                // the same idempotency key between our check and SaveChanges.
                await _unitOfWork.SaveChangesAsync(
                    cancellationToken);
            }
            catch (DuplicateIdempotencyKeyException)
            {
                // A concurrent request created the idempotency record. Load it and
                // return its response if it matches the current request.
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

        // Creates a deterministic hash for the withdrawal request. Currently
        // normalizes the amount and computes a SHA256 hex string.
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
