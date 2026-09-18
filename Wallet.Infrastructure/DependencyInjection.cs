using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Application.Abstractions;
using Wallet.Infrastructure.Events;
using Wallet.Infrastructure.Persistence;
using Wallet.Infrastructure.Repositories;

namespace Wallet.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<WalletDbContext>(options =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString(
                        "WalletDatabase"));
            });

            services.AddScoped<
                IWalletAccountRepository,
                WalletAccountRepository>();

            services.AddScoped<IEventPublisher,InMemoryEventPublisher>();

            services.AddScoped<IUnitOfWork, WalletUnitOfWork>();

            return services;
        }
    }
}
