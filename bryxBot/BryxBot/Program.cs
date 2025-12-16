using BryxBot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Маппинг переменных окружения для Railway
var botToken = Environment.GetEnvironmentVariable("BOT_TOKEN");
var crmApiUrl = Environment.GetEnvironmentVariable("CRM_API_URL");
var allowedUsers = Environment.GetEnvironmentVariable("ALLOWED_USERS");

if (!string.IsNullOrEmpty(botToken))
{
    builder.Configuration["BotConfiguration:BotToken"] = botToken;
    Console.WriteLine("✅ BOT_TOKEN loaded from environment");
}
else
{
    Console.WriteLine("⚠️ BOT_TOKEN not found in environment variables");
}

if (!string.IsNullOrEmpty(crmApiUrl))
{
    builder.Configuration["BotConfiguration:CrmApiUrl"] = crmApiUrl;
    Console.WriteLine($"✅ CRM_API_URL loaded from environment: {crmApiUrl}");
}
else
{
    Console.WriteLine("⚠️ CRM_API_URL not found in environment variables");
}

if (!string.IsNullOrEmpty(allowedUsers))
{
    // Парсим список пользователей через запятую
    var usersList = allowedUsers.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    for (int i = 0; i < usersList.Length; i++)
    {
        builder.Configuration[$"BotConfiguration:AllowedUsers:{i}"] = usersList[i];
    }
    Console.WriteLine($"✅ ALLOWED_USERS loaded from environment: {string.Join(", ", usersList)}");
}
else
{
    Console.WriteLine("⚠️ ALLOWED_USERS not found in environment variables - will allow all users if BotUsers table is empty");
}

builder.Services.Configure<BotConfiguration>(
    builder.Configuration.GetSection("BotConfiguration"));

builder.Services.AddHttpClient();
builder.Services.AddSingleton<MessageHandler>();
builder.Services.AddHostedService<BotService>();

var host = builder.Build();
await host.RunAsync();
