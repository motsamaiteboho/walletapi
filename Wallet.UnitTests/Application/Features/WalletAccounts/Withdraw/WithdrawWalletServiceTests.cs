using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Application.Abstractions;
using Wallet.Application.Events;
using Wallet.Application.Features.WalletAccounts.Withdraw;
using Wallet.Domain.Entities;
using Wallet.Domain.Exceptions;

namespace Wallet.UnitTests.Application.Features.WalletAccounts.Withdraw
{
    public class WithdrawWalletServiceTests
    {
        private static readonly Guid WalletId =
            Guid.Parse("11111111-1111-1111-1111-111111111111");

        [Fact]
        public async Task ExecuteAsync_WithSufficientFunds_WithdrawsAndPublishesEvent()
        {
            // Arrange
            var wallet = new WalletAccount(
                WalletId,
                1000.00m,
                "ZAR");

            var repository = new Mock<IWalletAccountRepository>();

            repository
                .Setup(x => x.GetByIdAsync(
                    WalletId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(wallet);

            var eventPublisher = new Mock<IEventPublisher>();

            var service = new WithdrawWalletService(
                repository.Object,
                eventPublisher.Object);

            var request = new WithdrawWalletRequest(250.00m);

            // Act
            var result = await service.ExecuteAsync(
                WalletId,
                request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(WalletId, result.WalletAccountId);
            Assert.Equal(250.00m, result.WithdrawnAmount);
            Assert.Equal(750.00m, result.RemainingBalance);
            Assert.Equal("ZAR", result.Currency);

            repository.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);

            eventPublisher.Verify(
                x => x.PublishAsync(
                    It.IsAny<WalletWithdrawalEvent>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithSufficientFunds_PublishesCorrectWithdrawalEvent()
        {
            // Arrange
            var wallet = new WalletAccount(
                WalletId,
                1000.00m,
                "ZAR");

            var repository = new Mock<IWalletAccountRepository>();

            repository
                .Setup(x => x.GetByIdAsync(
                    WalletId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(wallet);

            var eventPublisher = new Mock<IEventPublisher>();

            var service = new WithdrawWalletService(
                repository.Object,
                eventPublisher.Object);

            var request = new WithdrawWalletRequest(300.00m);

            // Act
            await service.ExecuteAsync(
                WalletId,
                request);

            // Assert
            eventPublisher.Verify(
                x => x.PublishAsync(
                    It.Is<WalletWithdrawalEvent>(e =>
                        e.WalletAccountId == WalletId &&
                        e.Amount == 300.00m &&
                        e.RemainingBalance == 700.00m &&
                        e.Currency == "ZAR"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithInsufficientFunds_DoesNotSaveOrPublishEvent()
        {
            // Arrange
            var wallet = new WalletAccount(
                WalletId,
                1000.00m,
                "ZAR");

            var repository = new Mock<IWalletAccountRepository>();

            repository
                .Setup(x => x.GetByIdAsync(
                    WalletId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(wallet);

            var eventPublisher = new Mock<IEventPublisher>();

            var service = new WithdrawWalletService(
                repository.Object,
                eventPublisher.Object);

            var request = new WithdrawWalletRequest(1500.00m);

            // Act
            await Assert.ThrowsAsync<InsufficientFundsException>(
                () => service.ExecuteAsync(
                    WalletId,
                    request));

            // Assert
            Assert.Equal(1000.00m, wallet.Balance);

            repository.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);

            eventPublisher.Verify(
                x => x.PublishAsync(
                    It.IsAny<WalletWithdrawalEvent>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_WhenWalletDoesNotExist_ReturnsNull()
        {
            // Arrange
            var repository = new Mock<IWalletAccountRepository>();

            repository
                .Setup(x => x.GetByIdAsync(
                    WalletId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((WalletAccount?)null);

            var eventPublisher = new Mock<IEventPublisher>();

            var service = new WithdrawWalletService(
                repository.Object,
                eventPublisher.Object);

            var request = new WithdrawWalletRequest(250.00m);

            // Act
            var result = await service.ExecuteAsync(
                WalletId,
                request);

            // Assert
            Assert.Null(result);

            repository.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);

            eventPublisher.Verify(
                x => x.PublishAsync(
                    It.IsAny<WalletWithdrawalEvent>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
