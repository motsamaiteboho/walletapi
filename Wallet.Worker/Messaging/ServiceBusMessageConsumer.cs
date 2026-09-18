using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Wallet.Application.Events;
using Wallet.Application.Features.WalletAccounts.Withdraw;
using Wallet.Application.Features.WalletAccounts.Withdraw.Payment;

namespace Wallet.Worker.Messaging
{

    public class ServiceBusMessageConsumer
    {
        private const string QueueName = "wallet-withdrawals";
        private readonly ServiceBusClient _client;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ServiceBusMessageConsumer> _logger;

        public ServiceBusMessageConsumer(
            ServiceBusClient client,
            IServiceScopeFactory scopeFactory,
            ILogger<ServiceBusMessageConsumer> logger)
        {
            _client = client;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task StartAsync(
            CancellationToken cancellationToken)
        {
            await using var processor =
                _client.CreateProcessor(
                    QueueName,
                    new ServiceBusProcessorOptions
                    {
                        AutoCompleteMessages = false,
                        MaxConcurrentCalls = 1
                    });

            processor.ProcessMessageAsync +=
                ProcessMessageAsync;

            processor.ProcessErrorAsync +=
                ProcessErrorAsync;

            await processor.StartProcessingAsync(
                cancellationToken);

            _logger.LogInformation(
                "Service Bus consumer started. Queue={QueueName}",
                QueueName);

            try
            {
                await Task.Delay(
                    Timeout.Infinite,
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when the worker shuts down.
            }

            await processor.StopProcessingAsync(
                CancellationToken.None);
        }

        private async Task ProcessMessageAsync( ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();

            var withdrawalEvent = JsonSerializer.Deserialize<WalletWithdrawalEvent>(body);

            if (withdrawalEvent is null)
            {
                throw new InvalidOperationException(
                    "Unable to deserialize WalletWithdrawalEvent.");
            }

            using var scope = _scopeFactory.CreateScope();

            var handler = scope.ServiceProvider
                .GetRequiredService<WithdrawalEventHandler>();

            try
            {
                await handler.HandleAsync(
                    withdrawalEvent,
                    args.CancellationToken);

                await args.CompleteMessageAsync(args.Message);
            }
            catch (PaymentValidationException exception)
            {
                await args.DeadLetterMessageAsync(
                    args.Message,
                    "PaymentValidationFailure",
                    exception.Message);
            }
            catch (PaymentProcessingException)
            {
                throw;
            }
        }

        private Task ProcessErrorAsync(
            ProcessErrorEventArgs args)
        {
            _logger.LogError(
                args.Exception,
                "Service Bus processing error. Entity={EntityPath}",
                args.EntityPath);

            return Task.CompletedTask;
        }
    }
}
