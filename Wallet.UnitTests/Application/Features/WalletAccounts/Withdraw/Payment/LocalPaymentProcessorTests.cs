using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Application.Events;
using Wallet.Application.Features.WalletAccounts.Withdraw.Payment;
using Wallet.Domain.Entities;

namespace Wallet.UnitTests.Application.Features.WalletAccounts.Withdraw.Payment
{
    public class LocalPaymentProcessorTests
    {
        [Fact]
        public async Task ProcessAsync_WhenEventAlreadyExists_DoesNotProcessAgain()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            var existingRecord = new PaymentProcessingRecord(
                eventId,
                Guid.NewGuid(),
                100m);

            existingRecord.MarkAsProcessed();

            var repository = new Mock<IPaymentProcessingRepository>();

            repository
                .Setup(x => x.GetByEventIdAsync(
                    eventId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingRecord);

            var logger =
                new Mock<ILogger<LocalPaymentProcessor>>();

            var processor = new LocalPaymentProcessor(
                repository.Object,
                logger.Object);

            var withdrawalEvent = new WalletWithdrawalEvent(
                Guid.NewGuid(),
                100m,
                900m,
                "ZAR",
                DateTime.UtcNow);

            // Act
            await processor.ProcessAsync(
                withdrawalEvent,
                eventId);

            // Assert
            repository.Verify(
                x => x.AddAsync(
                    It.IsAny<PaymentProcessingRecord>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            repository.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ProcessAsync_WhenEventDoesNotExist_CreatesAndProcessesRecord()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var walletAccountId = Guid.NewGuid();

            var repository =
                new Mock<IPaymentProcessingRepository>();

            repository
                .Setup(x => x.GetByEventIdAsync(
                    eventId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((PaymentProcessingRecord?)null);

            var logger =
                new Mock<ILogger<LocalPaymentProcessor>>();

            var processor = new LocalPaymentProcessor(
                repository.Object,
                logger.Object);

            var withdrawalEvent = new WalletWithdrawalEvent(
                walletAccountId,
                100m,
                900m,
                "ZAR",
                DateTime.UtcNow);

            // Act
            await processor.ProcessAsync(
                withdrawalEvent,
                eventId);

            // Assert
            repository.Verify(
                x => x.AddAsync(
                    It.Is<PaymentProcessingRecord>(record =>
                        record.EventId == eventId &&
                        record.WalletAccountId == walletAccountId &&
                        record.Amount == 100m),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            repository.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task ProcessAsync_WhenEventIsDifferent_ProcessesNewPayment()
        {
            // Arrange
            var firstEventId = Guid.NewGuid();
            var secondEventId = Guid.NewGuid();

            var repository =
                new Mock<IPaymentProcessingRepository>();

            repository
                .Setup(x => x.GetByEventIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((PaymentProcessingRecord?)null);

            var logger =
                new Mock<ILogger<LocalPaymentProcessor>>();

            var processor = new LocalPaymentProcessor(
                repository.Object,
                logger.Object);

            var withdrawalEvent = new WalletWithdrawalEvent(
                Guid.NewGuid(),
                100m,
                900m,
                "ZAR",
                DateTime.UtcNow);

            // Act
            await processor.ProcessAsync(
                withdrawalEvent,
                firstEventId);

            await processor.ProcessAsync(
                withdrawalEvent,
                secondEventId);

            // Assert
            repository.Verify(
                x => x.AddAsync(
                    It.IsAny<PaymentProcessingRecord>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }
    }
}
