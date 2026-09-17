using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Domain.Entities;

namespace Wallet.UnitTests.Domain.Entities
{
    public class WalletAccountTests
    {
        private static readonly Guid WalletId =
            Guid.Parse("11111111-1111-1111-1111-111111111111");

        [Fact]
        public void Withdraw_WithSufficientFunds_DecreasesBalance()
        {
            // Arrange
            var wallet = new WalletAccount(
                WalletId,
                1000.00m,
                "ZAR");

            // Act
            wallet.Withdraw(250.00m);

            // Assert
            Assert.Equal(750.00m, wallet.Balance);
        }

        [Fact]
        public void Withdraw_WithExactBalance_SetsBalanceToZero()
        {
            // Arrange
            var wallet = new WalletAccount(
                WalletId,
                1000.00m,
                "ZAR");

            // Act
            wallet.Withdraw(1000.00m);

            // Assert
            Assert.Equal(0.00m, wallet.Balance);
        }

        [Fact]
        public void Withdraw_WithInsufficientFunds_ThrowsException()
        {
            // Arrange
            var wallet = new WalletAccount(
                WalletId,
                1000.00m,
                "ZAR");

            // Act
            var exception = Assert.Throws<InvalidOperationException>(
                () => wallet.Withdraw(1000.01m));

            // Assert
            Assert.Equal(
                "Insufficient funds.",
                exception.Message);
        }

        [Fact]
        public void Withdraw_WithZeroAmount_ThrowsException()
        {
            // Arrange
            var wallet = new WalletAccount(
                WalletId,
                1000.00m,
                "ZAR");

            // Act
            var exception = Assert.Throws<ArgumentException>(
                () => wallet.Withdraw(0));

            // Assert
            Assert.Equal("amount", exception.ParamName);
        }

        [Fact]
        public void Withdraw_WithNegativeAmount_ThrowsException()
        {
            // Arrange
            var wallet = new WalletAccount(
                WalletId,
                1000.00m,
                "ZAR");

            // Act
            var exception = Assert.Throws<ArgumentException>(
                () => wallet.Withdraw(-100.00m));

            // Assert
            Assert.Equal("amount", exception.ParamName);
        }

        [Fact]
        public void Withdraw_WhenRejected_DoesNotChangeBalance()
        {
            // Arrange
            var wallet = new WalletAccount(
                WalletId,
                1000.00m,
                "ZAR");

            // Act
            Assert.Throws<InvalidOperationException>(
                () => wallet.Withdraw(1500.00m));

            // Assert
            Assert.Equal(1000.00m, wallet.Balance);
        }
    }
}
