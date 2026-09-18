using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using Wallet.Application.Events;
using Wallet.Application.Features.WalletAccounts.Withdraw;
using Wallet.Domain.Enums;
using Wallet.Infrastructure.Persistence;
using Wallet.Domain.Entities;

namespace Wallet.IntegrationTests
{
    public class WalletApiTests
    {

        private static async Task<Guid> CreateTestWalletAsync(
            WalletApiFactory factory,
            decimal initialBalance)
        {
            using var scope = factory.Services.CreateScope();

            var dbContext = scope.ServiceProvider
                .GetRequiredService<WalletDbContext>();

            var walletId = Guid.NewGuid();

            var wallet = new WalletAccount(
                walletId,
                initialBalance,
                "ZAR");

            dbContext.WalletAccounts.Add(wallet);

            await dbContext.SaveChangesAsync();

            return walletId;
        }

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

        [Fact]
        public async Task Withdraw_WithSufficientFunds_ReturnsOk()
        {
            await using var factory = new WalletApiFactory();
            var client = factory.CreateClient();

            var walletId =
                Guid.Parse("11111111-1111-1111-1111-111111111111");

            var request = new
            {
                amount = 1.00m
            };

            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                $"/api/wallet-accounts/{walletId}/withdraw");

            httpRequest.Headers.Add(
                "Idempotency-Key",
                $"integration-test-{Guid.NewGuid()}");

            httpRequest.Content = JsonContent.Create(request);

            var response = await client.SendAsync(httpRequest);

            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);
        }

        [Fact]
        public async Task Withdraw_WithInsufficientFunds_ReturnsUnprocessableEntity()
        {
            await using var factory = new WalletApiFactory();
            var client = factory.CreateClient();

            var walletId =
                Guid.Parse("11111111-1111-1111-1111-111111111111");

            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                $"/api/wallet-accounts/{walletId}/withdraw");

            httpRequest.Headers.Add(
                "Idempotency-Key",
                $"integration-test-{Guid.NewGuid()}");

            httpRequest.Content = JsonContent.Create(
                new
                {
                    amount = 999999999.00m
                });

            var response = await client.SendAsync(httpRequest);

            Assert.Equal(
                HttpStatusCode.UnprocessableEntity,
                response.StatusCode);
        }

        [Fact]
        public async Task Withdraw_WithMissingWallet_ReturnsNotFound()
        {
            await using var factory = new WalletApiFactory();
            var client = factory.CreateClient();

            var walletId = Guid.NewGuid();

            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                $"/api/wallet-accounts/{walletId}/withdraw");

            httpRequest.Headers.Add(
                "Idempotency-Key",
                $"integration-test-{Guid.NewGuid()}");

            httpRequest.Content = JsonContent.Create(
                new
                {
                    amount = 100.00m
                });

            var response = await client.SendAsync(httpRequest);

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);
        }

        [Fact]
        public async Task Withdraw_WithSameIdempotencyKey_ReturnsSameResult()
        {
            await using var factory = new WalletApiFactory();
            var client = factory.CreateClient();

            var walletId =
                Guid.Parse("11111111-1111-1111-1111-111111111111");

            var idempotencyKey =
                $"integration-test-{Guid.NewGuid()}";

            async Task<HttpResponseMessage> SendWithdrawal()
            {
                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"/api/wallet-accounts/{walletId}/withdraw");

                request.Headers.Add(
                    "Idempotency-Key",
                    idempotencyKey);

                request.Content = JsonContent.Create(
                    new
                    {
                        amount = 1.00m
                    });

                return await client.SendAsync(request);
            }

            var firstResponse = await SendWithdrawal();
            var secondResponse = await SendWithdrawal();

            Assert.Equal(
                HttpStatusCode.OK,
                firstResponse.StatusCode);

            Assert.Equal(
                HttpStatusCode.OK,
                secondResponse.StatusCode);

            var firstBody =
                await firstResponse.Content
                    .ReadFromJsonAsync<WithdrawResponse>();

            var secondBody =
                await secondResponse.Content
                    .ReadFromJsonAsync<WithdrawResponse>();

            Assert.NotNull(firstBody);
            Assert.NotNull(secondBody);

            Assert.Equal(
                firstBody.RemainingBalance,
                secondBody.RemainingBalance);

            Assert.Equal(
                firstBody.WithdrawnAmount,
                secondBody.WithdrawnAmount);
        }

        [Fact]
        public async Task Withdraw_Successfully_PersistsBalanceTransactionOutboxAndIdempotencyRecord()
        {
            await using var factory = new WalletApiFactory();

            var walletId = await CreateTestWalletAsync(
                factory,
                1000.00m);

            var client = factory.CreateClient();

            var idempotencyKey =
                $"integration-{Guid.NewGuid()}";

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/api/wallet-accounts/{walletId}/withdraw");

            request.Headers.Add(
                "Idempotency-Key",
                idempotencyKey);

            request.Content = JsonContent.Create(
                new
                {
                    amount = 100.00m
                });

            var response = await client.SendAsync(request);

            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);

            using var scope =
                factory.Services.CreateScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<WalletDbContext>();

            var wallet =
                await dbContext.WalletAccounts
                    .AsNoTracking()
                    .SingleAsync(x => x.Id == walletId);

            var transaction =
                await dbContext.WalletTransactions
                    .AsNoTracking()
                    .SingleAsync(x =>
                        x.WalletAccountId == walletId);

            var outboxMessage =
                await dbContext.OutboxMessages
                    .AsNoTracking()
                    .SingleAsync(x =>
                        x.Type == nameof(WalletWithdrawalEvent) &&
                        x.Payload.Contains(walletId.ToString()));

            var idempotencyRecord =
                await dbContext.IdempotencyRecords
                    .AsNoTracking()
                    .SingleAsync(x =>
                        x.WalletAccountId == walletId &&
                        x.IdempotencyKey == idempotencyKey);

            Assert.Equal(
                900.00m,
                wallet.Balance);

            Assert.Equal(
                100.00m,
                transaction.Amount);

            Assert.Equal(
                900.00m,
                transaction.BalanceAfter);

            Assert.Equal(
                WalletTransactionType.Withdrawal,
                transaction.Type);

            Assert.NotNull(outboxMessage.Payload);

            Assert.Equal(
                idempotencyKey,
                idempotencyRecord.IdempotencyKey);
        }

        private sealed record WithdrawResponse(
            Guid WalletAccountId,
            decimal WithdrawnAmount,
            decimal RemainingBalance,
            string Currency);
    }
}
