using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Infrastructure.Persistence.Idempotency
{
    public class IdempotencyRecordConfiguration
    : IEntityTypeConfiguration<IdempotencyRecordEntity>
    {
        public void Configure(
            EntityTypeBuilder<IdempotencyRecordEntity> builder)
        {
            builder.ToTable("idempotency_records");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.IdempotencyKey)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.RequestHash)
                .HasMaxLength(128)
                .IsRequired();

            builder.Property(x => x.ResponsePayload)
                .IsRequired();

            builder.Property(x => x.StatusCode)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasIndex(
                x => new
                {
                    x.WalletAccountId,
                    x.IdempotencyKey
                })
            .IsUnique()
            .HasDatabaseName("ux_idempotency_wallet_key");
        }
    }
}
