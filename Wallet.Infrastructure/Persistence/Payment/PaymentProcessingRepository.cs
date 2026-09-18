using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Application.Features.WalletAccounts.Withdraw.Payment;
using Wallet.Domain.Entities;

namespace Wallet.Infrastructure.Persistence.Payment
{
    public class PaymentProcessingRepository
    : IPaymentProcessingRepository
    {
        private readonly WalletDbContext _dbContext;

        public PaymentProcessingRepository(
            WalletDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<PaymentProcessingRecord?> GetByEventIdAsync(
            Guid eventId,
            CancellationToken cancellationToken = default)
        {
            return _dbContext.PaymentProcessingRecords
                .FirstOrDefaultAsync(
                    x => x.EventId == eventId,
                    cancellationToken);
        }

        public async Task AddAsync(
            PaymentProcessingRecord record,
            CancellationToken cancellationToken = default)
        {
            await _dbContext.PaymentProcessingRecords.AddAsync(
                record,
                cancellationToken);
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
