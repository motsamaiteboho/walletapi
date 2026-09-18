using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Application.Events;
using Wallet.Application.Features.WalletAccounts.Withdraw;
using Wallet.Application.Features.WalletAccounts.Withdraw.Payment;

namespace Wallet.UnitTests.Application.Features.WalletAccounts.Withdraw
{
    public class WithdrawalEventHandlerTests
    {
        private readonly Mock<IWithdrawalProcessor> _processorMock;
        private readonly Mock<ILogger<WithdrawalEventHandler>> _loggerMock;
        private readonly Mock<ILogger<ProcessWithdrawalEventService>> _serviceLoggerMock;

        public WithdrawalEventHandlerTests()
        {
            _processorMock = new Mock<IWithdrawalProcessor>();
            _loggerMock = new Mock<ILogger<WithdrawalEventHandler>>();
            _serviceLoggerMock = new Mock<ILogger<ProcessWithdrawalEventService>>();
        }

        [Fact]
        public async Task HandleAsync_WhenProcessingSucceeds_CompletesSuccessfully()
        {
            // Arrange
            var processService = new ProcessWithdrawalEventService(
                _processorMock.Object);

            var handler = new WithdrawalEventHandler(
                processService,
                _loggerMock.Object);

            var withdrawalEvent = CreateWithdrawalEvent();

            // Act
            await handler.HandleAsync(withdrawalEvent);

            // Assert
            _processorMock.Verify(
                x => x.ProcessAsync(
                    withdrawalEvent,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task HandleAsync_WhenPaymentProcessingFails_PropagatesException()
        {
            // Arrange
            _processorMock
                .Setup(x => x.ProcessAsync(
                    It.IsAny<WalletWithdrawalEvent>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(
                    new PaymentProcessingException(
                        "Payment service unavailable."));

            var processService = new ProcessWithdrawalEventService(
                _processorMock.Object);

            var handler = new WithdrawalEventHandler(
                processService,
                _loggerMock.Object);

            var withdrawalEvent = CreateWithdrawalEvent();

            // Act & Assert
            await Assert.ThrowsAsync<PaymentProcessingException>(
                () => handler.HandleAsync(withdrawalEvent));
        }

        [Fact]
        public async Task HandleAsync_WhenPaymentValidationFails_PropagatesException()
        {
            // Arrange
            _processorMock
                .Setup(x => x.ProcessAsync(
                    It.IsAny<WalletWithdrawalEvent>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(
                    new PaymentValidationException(
                        "Invalid payment details."));

            var processService = new ProcessWithdrawalEventService(
                _processorMock.Object);

            var handler = new WithdrawalEventHandler(
                processService,
                _loggerMock.Object);

            var withdrawalEvent = CreateWithdrawalEvent();

            // Act & Assert
            await Assert.ThrowsAsync<PaymentValidationException>(
                () => handler.HandleAsync(withdrawalEvent));
        }

        private static WalletWithdrawalEvent CreateWithdrawalEvent()
        {
            return new WalletWithdrawalEvent(
                Guid.NewGuid(),
                100m,
                900m,
                "ZAR",
                DateTime.UtcNow);
        }
    }
}
