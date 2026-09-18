using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Domain.Entities;

namespace Wallet.Application.Features.WalletAccounts.Withdraw.Payment
{
    public interface IPaymentProcessingRepository
    {
        Task<PaymentProcessingRecord?> GetByEventIdAsync(
            Guid eventId,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            PaymentProcessingRecord record,
            CancellationToken cancellationToken = default);

        Task SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
