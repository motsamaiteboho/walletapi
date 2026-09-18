using Azure.Messaging.ServiceBus;
using Wallet.Application;
using Wallet.Infrastructure;
using Wallet.Worker;
using Wallet.Worker.Messaging;
using Wallet.Worker.Outbox;

var builder = Host.CreateApplicationBuilder(args);

var serviceBusConnectionString =
    builder.Configuration["ServiceBus:ConnectionString"];

builder.Services.AddApplication();

builder.Services.AddInfrastructure(
    builder.Configuration);

builder.Services.AddSingleton(
    new ServiceBusClient(serviceBusConnectionString));

builder.Services.AddSingleton<ServiceBusMessageSender>();

builder.Services.AddSingleton<ServiceBusMessageConsumer>();

builder.Services.AddHostedService<Worker>();

builder.Services.AddHostedService<OutboxPublisherWorker>();

var host = builder.Build();

host.Run();