using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Worker.Messaging
{
    public class ServiceBusFailureTest
    {
        private const string QueueName = "wallet-withdrawals";

        private readonly ServiceBusClient _client;

        public ServiceBusFailureTest(ServiceBusClient client)
        {
            _client = client;
        }

        public async Task SendFailureTestAsync()
        {
            await using var sender = _client.CreateSender(QueueName);

            var message = new ServiceBusMessage("""
        {
            "WalletAccountId": "11111111-1111-1111-1111-111111111111",
            "Amount": 9999,
            "RemainingBalance": 1,
            "Currency": "ZAR",
            "OccurredAt": "2026-09-18T10:00:00Z"
        }
        """)
            {
                MessageId = Guid.NewGuid().ToString(),
                Subject = "WalletWithdrawalEvent",
                ContentType = "application/json"
            };

            await sender.SendMessageAsync(message);
        }
    }
}
