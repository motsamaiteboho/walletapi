using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Application.Abstractions;
using Wallet.Application.Events;
using Wallet.Application.Exceptions;
using Wallet.Application.Features.WalletAccounts.Withdraw;
using Wallet.Domain.Entities;
using Wallet.Domain.Exceptions;

namespace Wallet.UnitTests.Features.WalletAccounts.Withdraw
{
    public class WithdrawWalletServiceTests
    {
        private readonly Mock<IWalletAccountRepository>
            _walletRepository;

        private readonly Mock<IEventPublisher>
            _eventPublisher;

        private readonly Mock<IUnitOfWork>
            _unitOfWork;

        private readonly Mock<IIdempotencyRepository>
            _idempotencyRepository;

        private readonly Mock<ILogger<WithdrawWalletService>>
            _logger;

        private readonly WithdrawWalletService _service;

        public WithdrawWalletServiceTests()
        {
            _walletRepository = new Mock<IWalletAccountRepository>();
            _eventPublisher = new Mock<IEventPublisher>();
            _unitOfWork = new Mock<IUnitOfWork>();
            _idempotencyRepository = new Mock<IIdempotencyRepository>();
            _logger = new Mock<ILogger<WithdrawWalletService>>();

            _service = new WithdrawWalletService(
                _walletRepository.Object,
                _eventPublisher.Object,
                _unitOfWork.Object,
                _idempotencyRepository.Object,
                _logger.Object);
        }

        [Fact]
        public async Task Withdraw_WithSufficientFunds_WithdrawsSuccessfully()
        {
            // Arrange
            var walletId = Guid.NewGuid();

            var wallet = new WalletAccount(
                walletId,
                1000m,
                "ZAR");

            _walletRepository
                .Setup(x => x.GetByIdAsync(
                    walletId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(wallet);

            var request = new WithdrawWalletRequest(250m);

            // Act
            var result = await _service.ExecuteAsync(
                walletId,
                request,
                "withdrawal-001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(walletId, result.WalletAccountId);
            Assert.Equal(250m, result.WithdrawnAmount);
            Assert.Equal(750m, result.RemainingBalance);
            Assert.Equal("ZAR", result.Currency);
        }

        [Fact]
        public async Task Withdraw_WithExistingIdempotencyKey_ReturnsExistingResult()
        {
            // Arrange
            var walletId = Guid.NewGuid();

            var existingResponse =
                new WithdrawWalletResponse(
                    walletId,
                    250m,
                    750m,
                    "ZAR");

            var existingRecord = new IdempotencyRecord(
                walletId,
                "withdrawal-001",
                CreateRequestHash(250m),
                System.Text.Json.JsonSerializer.Serialize(
                    existingResponse),
                200,
                DateTime.UtcNow);

            _idempotencyRepository
                .Setup(x => x.GetAsync(
                    walletId,
                    "withdrawal-001",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingRecord);

            var request = new WithdrawWalletRequest(250m);

            // Act
            var result = await _service.ExecuteAsync(
                walletId,
                request,
                "withdrawal-001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(walletId, result.WalletAccountId);
            Assert.Equal(250m, result.WithdrawnAmount);
            Assert.Equal(750m, result.RemainingBalance);

            // The wallet should NOT be retrieved.
            _walletRepository.Verify(
                x => x.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            // No second event.
            _eventPublisher.Verify(
                x => x.PublishAsync(
                    It.IsAny<WalletWithdrawalEvent>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            // No second database save.
            _unitOfWork.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Withdraw_WithSameKeyAndDifferentAmount_ThrowsConflict()
        {
            // Arrange
            var walletId = Guid.NewGuid();

            var existingResponse =
                new WithdrawWalletResponse(
                    walletId,
                    250m,
                    750m,
                    "ZAR");

            var existingRecord = new IdempotencyRecord(
                walletId,
                "withdrawal-001",
                CreateRequestHash(250m),
                System.Text.Json.JsonSerializer.Serialize(
                    existingResponse),
                200,
                DateTime.UtcNow);

            _idempotencyRepository
                .Setup(x => x.GetAsync(
                    walletId,
                    "withdrawal-001",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingRecord);

            var request = new WithdrawWalletRequest(500m);

            // Act & Assert
            await Assert.ThrowsAsync<IdempotencyKeyConflictException>(
                () => _service.ExecuteAsync(
                    walletId,
                    request,
                    "withdrawal-001"));
        }

        [Fact]
        public async Task Withdraw_WithDifferentIdempotencyKeys_AllowsNewWithdrawal()
        {
            // Arrange
            var walletId = Guid.NewGuid();

            var wallet = new WalletAccount(
                walletId,
                1000m,
                "ZAR");

            _walletRepository
                .Setup(x => x.GetByIdAsync(
                    walletId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(wallet);

            // Act
            var firstResult = await _service.ExecuteAsync(
                walletId,
                new WithdrawWalletRequest(100m),
                "withdrawal-001");

            var secondResult = await _service.ExecuteAsync(
                walletId,
                new WithdrawWalletRequest(100m),
                "withdrawal-002");

            // Assert
            Assert.NotNull(firstResult);
            Assert.NotNull(secondResult);

            Assert.Equal(900m, firstResult.RemainingBalance);
            Assert.Equal(800m, secondResult.RemainingBalance);
        }

        [Fact]
        public async Task Withdraw_WithInsufficientFunds_ThrowsException()
        {
            // Arrange
            var walletId = Guid.NewGuid();

            var wallet = new WalletAccount(
                walletId,
                100m,
                "ZAR");

            _walletRepository
                .Setup(x => x.GetByIdAsync(
                    walletId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(wallet);

            // Act & Assert
            await Assert.ThrowsAsync<InsufficientFundsException>(
                () => _service.ExecuteAsync(
                    walletId,
                    new WithdrawWalletRequest(150m),
                    "withdrawal-001"));

            // Nothing should be persisted.
            _unitOfWork.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _eventPublisher.Verify(
                x => x.PublishAsync(
                    It.IsAny<WalletWithdrawalEvent>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _idempotencyRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<IdempotencyRecord>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Withdraw_WhenWalletDoesNotExist_ReturnsNull()
        {
            // Arrange
            var walletId = Guid.NewGuid();

            _walletRepository
                .Setup(x => x.GetByIdAsync(
                    walletId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((WalletAccount?)null);

            // Act
            var result = await _service.ExecuteAsync(
                walletId,
                new WithdrawWalletRequest(100m),
                "withdrawal-001");

            // Assert
            Assert.Null(result);

            _unitOfWork.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Withdraw_Successfully_StoresIdempotencyRecord()
        {
            // Arrange
            var walletId = Guid.NewGuid();

            var wallet = new WalletAccount(
                walletId,
                1000m,
                "ZAR");

            _walletRepository
                .Setup(x => x.GetByIdAsync(
                    walletId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(wallet);

            // Act
            await _service.ExecuteAsync(
                walletId,
                new WithdrawWalletRequest(250m),
                "withdrawal-001");

            // Assert
            _idempotencyRepository.Verify(
                x => x.AddAsync(
                    It.Is<IdempotencyRecord>(record =>
                        record.WalletAccountId == walletId &&
                        record.IdempotencyKey == "withdrawal-001" &&
                        record.StatusCode == 200),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Withdraw_Successfully_SavesChanges()
        {
            // Arrange
            var walletId = Guid.NewGuid();

            var wallet = new WalletAccount(
                walletId,
                1000m,
                "ZAR");

            _walletRepository
                .Setup(x => x.GetByIdAsync(
                    walletId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(wallet);

            // Act
            await _service.ExecuteAsync(
                walletId,
                new WithdrawWalletRequest(250m),
                "withdrawal-001");

            // Assert
            _unitOfWork.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        private static string CreateRequestHash( decimal amount)
        {
            var normalizedAmount =
                amount.ToString(
                    "0.00",
                    System.Globalization.CultureInfo.InvariantCulture);

            var bytes =
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(
                        normalizedAmount));

            return Convert.ToHexString(bytes);
        }

    }
}
