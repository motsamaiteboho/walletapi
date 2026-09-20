using Wallet.Worker.Messaging;

namespace Wallet.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ServiceBusMessageSender _sender;
        private readonly ServiceBusMessageConsumer _consumer;

        // Constructs the worker with required messaging dependencies.
        public Worker(
            ILogger<Worker> logger,
            ServiceBusMessageSender sender,
            ServiceBusMessageConsumer consumer)
        {
            _logger = logger;
            _sender = sender;
            _consumer = consumer;
        }

        // Main execution entry for the BackgroundService. Starts the consumer and
        // keeps the background task alive until cancellation is requested.
        protected override async Task ExecuteAsync( CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Wallet Worker started.");

            await _consumer.StartAsync(
                stoppingToken);
        }
    }
}
