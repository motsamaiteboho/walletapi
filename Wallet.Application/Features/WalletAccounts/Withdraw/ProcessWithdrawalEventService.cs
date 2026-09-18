using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Application.Events;

namespace Wallet.Application.Features.WalletAccounts.Withdraw
{
    public class ProcessWithdrawalEventService
    {
        private readonly IWithdrawalProcessor _processor;

        public ProcessWithdrawalEventService(
            IWithdrawalProcessor processor)
        {
            _processor = processor;
        }

        public Task ProcessAsync(
            WalletWithdrawalEvent withdrawalEvent,
            Guid eventId,
            CancellationToken cancellationToken = default)
        {
            return _processor.ProcessAsync(
                withdrawalEvent,
                eventId,
                cancellationToken);
        }
    }
}
