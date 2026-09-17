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
    public class WalletAccountConfiguration
    : IEntityTypeConfiguration<WalletAccount>
    {
        public void Configure(
            EntityTypeBuilder<WalletAccount> builder)
        {
            builder.ToTable("wallet_accounts");

            builder.HasKey(account => account.Id);

            builder.Property(account => account.Balance)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(account => account.Currency)
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(account => account.CreatedAt)
                .IsRequired();
        }
    }
}
