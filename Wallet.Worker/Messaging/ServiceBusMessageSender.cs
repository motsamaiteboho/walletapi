using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Worker.Messaging
{
    public class ServiceBusMessageSender
    {
        private const string QueueName =
            "wallet-withdrawals";

        private readonly ServiceBusClient _client;

        public ServiceBusMessageSender(
            ServiceBusClient client)
        {
            _client = client;
        }

        public async Task SendAsync(
            string payload,
            Guid messageId,
            string messageType,
            CancellationToken cancellationToken = default)
        {
            await using var sender =
                _client.CreateSender(QueueName);

            var message =
                new ServiceBusMessage(payload)
                {
                    MessageId = messageId.ToString(),
                    Subject = messageType,
                    ContentType = "application/json"
                };

            message.ApplicationProperties["EventType"] =
                messageType;

            await sender.SendMessageAsync(
                message,
                cancellationToken);
        }
    }
}
