using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Domain.Entities;

namespace Wallet.Infrastructure.Persistence
{
    public static class WalletDbSeeder
    {
        private static readonly Guid InitialWalletId =
            Guid.Parse("11111111-1111-1111-1111-111111111111");

        public static async Task SeedAsync(WalletDbContext context)
        {
            var exists = await context.WalletAccounts
                .AnyAsync(x => x.Id == InitialWalletId);

            if (exists)
            {
                return;
            }

            var walletAccount = new WalletAccount(
                InitialWalletId,
                10_000.00m,
                "ZAR");

            context.WalletAccounts.Add(walletAccount);

            await context.SaveChangesAsync();
        }
    }
}
