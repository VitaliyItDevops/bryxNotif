using bryx_CRM.Components;
using bryx_CRM.Data;
using bryx_CRM.Data.Models;
using bryx_CRM.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Настройка порта для Railway (или другого облачного провайдера)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5092";
Console.WriteLine($"🚀 Starting application on port: {port}");
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Настройка Forwarded Headers для работы за HTTPS прокси (Railway, Nginx, и т.д.)
// Railway терминирует SSL и отправляет заголовки X-Forwarded-Proto: https
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Доверяем всем прокси (безопасно для Railway, так как это закрытая сеть)
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Добавляем поддержку API контроллеров для бота
builder.Services.AddControllers();

// Добавляем HttpClient для Telegram уведомлений
builder.Services.AddHttpClient();

// Настройка подключения к PostgreSQL через PooledDbContextFactory
// Railway автоматически предоставляет DATABASE_URL
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

Console.WriteLine($"🔍 DATABASE_URL environment variable: {(Environment.GetEnvironmentVariable("DATABASE_URL") != null ? "EXISTS" : "NOT FOUND")}");
Console.WriteLine($"🔍 Using connection string format: {(connectionString?.StartsWith("postgres") == true ? "Railway format (postgres://)" : "Npgsql format")}");

// Преобразование DATABASE_URL формата Railway в ConnectionString для Npgsql
// Railway может использовать postgres:// или postgresql://
if (connectionString?.StartsWith("postgres://") == true || connectionString?.StartsWith("postgresql://") == true)
{
    try
    {
        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':');
        connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
        Console.WriteLine($"✅ Converted Railway DATABASE_URL to Npgsql format");
        Console.WriteLine($"🔗 Host: {uri.Host}, Port: {uri.Port}, Database: {uri.AbsolutePath.TrimStart('/')}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error parsing DATABASE_URL: {ex.Message}");
        throw;
    }
}
else
{
    Console.WriteLine($"⚠️ Using local connection string (not Railway format)");
}

// Регистрация PooledDbContextFactory для Blazor компонентов
builder.Services.AddPooledDbContextFactory<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    // Подавляем предупреждение о pending model changes, так как изменения в названиях свойств
    // не влияют на схему БД (правильно маппятся через HasColumnName в BryxCrmContext)
    options.ConfigureWarnings(warnings =>
        warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// Регистрация DbContext для Identity
// Используем AddDbContextPool для совместимости с PooledDbContextFactory
builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    // Подавляем предупреждение о pending model changes
    options.ConfigureWarnings(warnings =>
        warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// Настройка ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Настройки пароля
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    // Настройки блокировки
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Настройки пользователя
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Настройка cookies для аутентификации
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

// Регистрация сервисов для бекапов
builder.Services.Configure<BackupOptions>(builder.Configuration.GetSection("BackupOptions"));

// Регистрация провайдеров бэкапов (порядок важен - проверяется по очереди)
// 1. Railway Volume Provider - приоритет на Railway
builder.Services.AddScoped<IBackupProvider, RailwayVolumeBackupProvider>();
// 2. Local Provider - fallback для локальной разработки
builder.Services.AddScoped<IBackupProvider, LocalBackupProvider>();

// Основной сервис бэкапов
builder.Services.AddScoped<BackupService>();

// Автоматические бэкапы
builder.Services.AddHostedService<AutoBackupHostedService>();

// Настройка Telegram конфигурации из переменных окружения
var telegramBotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
var telegramChatId = Environment.GetEnvironmentVariable("TELEGRAM_CHAT_ID");

if (!string.IsNullOrEmpty(telegramBotToken))
{
    builder.Configuration["TelegramBot:BotToken"] = telegramBotToken;
    Console.WriteLine("✅ TELEGRAM_BOT_TOKEN loaded from environment");
}
else
{
    Console.WriteLine("⚠️ TELEGRAM_BOT_TOKEN not found in environment variables");
}

if (!string.IsNullOrEmpty(telegramChatId))
{
    builder.Configuration["TelegramBot:ChatId"] = telegramChatId;
    Console.WriteLine("✅ TELEGRAM_CHAT_ID loaded from environment");
}
else
{
    Console.WriteLine("⚠️ TELEGRAM_CHAT_ID not found in environment variables");
}

// Telegram уведомления
builder.Services.AddScoped<TelegramNotificationService>();

var app = builder.Build();

// Автоматическое применение миграций при старте (для Railway и других cloud платформ)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Console.WriteLine("🔄 Начинаем применение миграций...");

        // Проверяем pending миграции
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        var pendingList = pendingMigrations.ToList();

        if (pendingList.Any())
        {
            Console.WriteLine($"📋 Найдено {pendingList.Count} неприменённых миграций:");
            foreach (var migration in pendingList)
            {
                Console.WriteLine($"   - {migration}");
            }
        }
        else
        {
            Console.WriteLine("ℹ️ Нет неприменённых миграций");
        }

        // Применить все pending миграции
        await context.Database.MigrateAsync();
        Console.WriteLine("✅ Миграции базы данных применены успешно");

        // Проверяем применённые миграции
        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
        Console.WriteLine($"✅ Всего применено миграций: {appliedMigrations.Count()}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ КРИТИЧЕСКАЯ ОШИБКА при применении миграций: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
        }
        throw; // Останавливаем приложение при ошибке миграций
    }
}

// Создание администратора по умолчанию
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var adminEmail = "admin@bryx.com";
    var adminPassword = "admin123";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Администратор",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            Console.WriteLine($"Создан пользователь администратора: {adminEmail} / {adminPassword}");
        }
    }
}

// Configure the HTTP request pipeline.

// ВАЖНО: UseForwardedHeaders должен быть первым!
// Это позволяет приложению понимать, что оно за HTTPS прокси
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found");

// HTTPS редирект не нужен - Railway уже редиректит на HTTPS на уровне прокси
// UseForwardedHeaders гарантирует, что приложение генерирует HTTPS ссылки
// app.UseHttpsRedirection();

// Middleware для аутентификации и авторизации
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Маппинг API контроллеров для бота
app.MapControllers();

app.Run();
