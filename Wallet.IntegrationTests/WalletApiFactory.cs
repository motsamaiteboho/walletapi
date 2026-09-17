using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Wallet.IntegrationTests
{
    public class WalletApiFactory
    : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(
            IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(
                (_, config) =>
                {
                    var settings = new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:WalletDatabase"] =
                            "Host=localhost;" +
                            "Port=5433;" +
                            "Database=wallet;" +
                            "Username=postgres;" +
                            "Password=postgres"
                    };

                    config.AddInMemoryCollection(settings);
                });
        }
    }
}
