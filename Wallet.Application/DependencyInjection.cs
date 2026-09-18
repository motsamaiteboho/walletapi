using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Application.Abstractions;
using Wallet.Application.Features.WalletAccounts.GetTransactions;
using Wallet.Application.Features.WalletAccounts.Withdraw;
using Wallet.Application.Features.WalletAccounts.Withdraw.Payment;

namespace Wallet.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services)
        {
            services.AddScoped<GetWalletBalanceService>();
            services.AddScoped<WithdrawWalletService>();
            services.AddScoped<GetWalletTransactionsService>();
            services.AddScoped<IWithdrawalProcessor, LocalWithdrawalProcessor>();
            services.AddScoped<ProcessWithdrawalEventService>();
            services.AddScoped<IWithdrawalProcessor, LocalWithdrawalProcessor>();
            services.AddScoped<IPaymentProcessor, LocalPaymentProcessor>();
            services.AddScoped<ProcessWithdrawalEventService>();
            services.AddScoped<WithdrawalEventHandler>();
            return services;
        }
    }
}
