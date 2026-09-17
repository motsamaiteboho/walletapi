using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using Wallet.Application.Features.WalletAccounts.Withdraw;

namespace Wallet.IntegrationTests
{
    public class WalletApiTests
    {
        [Fact]
        public async Task GetBalance_WithExistingWallet_ReturnsOk()
        {
            // Arrange
            await using var factory = new WalletApiFactory();

            var client = factory.CreateClient();

            var walletId =
                Guid.Parse(
                    "11111111-1111-1111-1111-111111111111");

            // Act
            var response = await client.GetAsync(
                $"/api/wallet-accounts/{walletId}/balance");

            // Assert
            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);
        }
    }
}
