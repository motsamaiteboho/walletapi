using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Infrastructure.Persistence.Outbox;

namespace Wallet.Infrastructure.Persistence.Configurations
{
    public class OutboxMessageConfiguration
    : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(
            EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("outbox_messages");

            builder.HasKey(message => message.Id);

            builder.Property(message => message.Type)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(message => message.Payload)
                .IsRequired();

            builder.Property(message => message.OccurredAt)
                .IsRequired();

            builder.Property(message => message.ProcessedAt);

            builder.Property(message => message.RetryCount)
                .IsRequired();

            builder.Property(message => message.Error);

            builder.HasIndex(message => message.ProcessedAt);

            builder.Property(message => message.NextRetryAt);

            builder.HasIndex(message => message.NextRetryAt);
        }
    }
}