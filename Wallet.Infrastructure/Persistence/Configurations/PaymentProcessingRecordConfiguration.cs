using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Domain.Entities;

namespace Wallet.Infrastructure.Persistence.Configurations
{
    public class PaymentProcessingRecordConfiguration
    : IEntityTypeConfiguration<PaymentProcessingRecord>
    {
        public void Configure(
            EntityTypeBuilder<PaymentProcessingRecord> builder)
        {
            builder.ToTable("payment_processing_records");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EventId)
                .IsRequired();

            builder.Property(x => x.WalletAccountId)
                .IsRequired();

            builder.Property(x => x.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasIndex(x => x.EventId)
                .IsUnique();
        }
    }
}
