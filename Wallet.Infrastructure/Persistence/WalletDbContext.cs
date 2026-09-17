using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Wallet.Domain.Entities;
using Entities = Wallet.Domain.Entities;

namespace Wallet.Infrastructure.Persistence
{
    public class WalletDbContext : DbContext
    {
        public WalletDbContext(
            DbContextOptions<WalletDbContext> options)
            : base(options)
        {
        }

        public DbSet<WalletAccount> WalletAccounts =>
            Set<WalletAccount>();

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(WalletDbContext).Assembly);
        }
    }
}
