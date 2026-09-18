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
    public class WalletTransactionConfiguration
    : IEntityTypeConfiguration<WalletTransaction>
    {
        public void Configure(
            EntityTypeBuilder<WalletTransaction> builder)
        {
            builder.ToTable("wallet_transactions");

            builder.HasKey(transaction => transaction.Id);

            builder.Property(transaction => transaction.WalletAccountId)
                .IsRequired();

            builder.Property(transaction => transaction.Type)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(transaction => transaction.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(transaction => transaction.BalanceAfter)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(transaction => transaction.CreatedAt)
                .IsRequired();

            builder.HasOne<WalletAccount>()
                .WithMany()
                .HasForeignKey(transaction =>
                    transaction.WalletAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(transaction =>
                new
                {
                    transaction.WalletAccountId,
                    transaction.CreatedAt
                });
        }
    }
}
