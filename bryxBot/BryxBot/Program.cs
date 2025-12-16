using BryxBot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.Configure<BotConfiguration>(
    builder.Configuration.GetSection("BotConfiguration"));

builder.Services.AddHttpClient();
builder.Services.AddSingleton<MessageHandler>();
builder.Services.AddHostedService<BotService>();

var host = builder.Build();
await host.RunAsync();
