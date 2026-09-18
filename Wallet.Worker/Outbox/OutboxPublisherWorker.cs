using Wallet.Infrastructure.Persistence.Outbox;
using Wallet.Worker.Messaging;

namespace Wallet.Worker.Outbox
{
    public class OutboxPublisherWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxPublisherWorker> _logger;

        public OutboxPublisherWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<OutboxPublisherWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Outbox publisher worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PublishPendingMessagesAsync(
                        stoppingToken);
                }
                catch (Exception exception)
                {
                    _logger.LogError(
                        exception,
                        "Error while publishing outbox messages.");
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    stoppingToken);
            }
        }

        private async Task PublishPendingMessagesAsync(
            CancellationToken cancellationToken)
        {
            using var scope =
                _scopeFactory.CreateScope();

            var repository =
                scope.ServiceProvider
                    .GetRequiredService<IOutboxMessageRepository>();

            var sender =
                scope.ServiceProvider
                    .GetRequiredService<ServiceBusMessageSender>();

            var messages =
                await repository.GetPendingAsync(
                    batchSize: 20,
                    cancellationToken);

            foreach (var message in messages)
            {
                try
                {
                    await sender.SendAsync(
                        message.Payload,
                        message.Id,
                        message.Type,
                        cancellationToken);

                    message.MarkAsProcessed();

                    await repository.SaveChangesAsync(
                        cancellationToken);

                    _logger.LogInformation(
                        "Outbox message published. " +
                        "OutboxMessageId={OutboxMessageId}, Type={Type}",
                        message.Id,
                        message.Type);
                }
                catch (Exception exception)
                {
                    _logger.LogError(
                        exception,
                        "Failed to publish outbox message. " +
                        "OutboxMessageId={OutboxMessageId}",
                        message.Id);

                    message.MarkAsFailed(
                        exception.Message,
                        TimeSpan.FromSeconds(10));

                    await repository.SaveChangesAsync(
                        cancellationToken);
                }
            }
        }
    }
}
