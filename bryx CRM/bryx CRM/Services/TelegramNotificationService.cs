using bryx_CRM.Data;
using bryx_CRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace bryx_CRM.Services;

public class TelegramNotificationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TelegramNotificationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public TelegramNotificationService(
        IHttpClientFactory httpClientFactory,
        ILogger<TelegramNotificationService> logger,
        IConfiguration configuration,
        IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
        _configuration = configuration;
        _dbContextFactory = dbContextFactory;
    }

    public async Task SendSaleNotification(Sale sale, List<Product> products)
    {
        try
        {
            var botToken = _configuration["TelegramBot:BotToken"];

            if (string.IsNullOrEmpty(botToken))
            {
                _logger.LogWarning("Telegram bot token is missing. Skipping notification.");
                return;
            }

            // Получаем всех подтвержденных пользователей из БД
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var confirmedUsers = await context.BotUsers
                .Where(u => u.IsActive && u.IsConfirmed && !string.IsNullOrEmpty(u.ChatId))
                .ToListAsync();

            if (!confirmedUsers.Any())
            {
                _logger.LogWarning("No confirmed bot users found. Skipping notification.");
                return;
            }

            _logger.LogInformation("Sending sale notification to {Count} confirmed users", confirmedUsers.Count);

            // Формируем сообщение
            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine("🛒 <b>Новая продажа!</b>");
            messageBuilder.AppendLine();
            messageBuilder.AppendLine($"👤 <b>Покупатель:</b> {sale.Buyer}");
            messageBuilder.AppendLine($"💰 <b>Сумма:</b> {sale.TotalAmount:N0} грн");
            messageBuilder.AppendLine($"📅 <b>Дата:</b> {sale.SaleDate:dd.MM.yyyy}");

            if (!string.IsNullOrEmpty(sale.TTN))
            {
                messageBuilder.AppendLine($"📦 <b>ТТН:</b> {sale.TTN}");
            }

            if (!string.IsNullOrEmpty(sale.SoldThrough))
            {
                messageBuilder.AppendLine($"🏪 <b>Продано через:</b> {sale.SoldThrough}");
            }

            messageBuilder.AppendLine();
            messageBuilder.AppendLine("<b>📦 Товары:</b>");

            foreach (var product in products)
            {
                var productLine = $"  • {product.Name}";
                if (!string.IsNullOrEmpty(product.Color))
                {
                    productLine += $" ({product.Color})";
                }
                productLine += $" - {product.SalePrice:N0} грн";
                messageBuilder.AppendLine(productLine);
            }

            // Создаем inline кнопку "Отправлено"
            var keyboard = new
            {
                inline_keyboard = new[]
                {
                    new[]
                    {
                        new
                        {
                            text = "📦 Отправлено",
                            callback_data = $"ship_{sale.Id}"
                        }
                    }
                }
            };

            // Отправляем сообщение всем подтвержденным пользователям
            var successCount = 0;
            var failCount = 0;

            foreach (var user in confirmedUsers)
            {
                try
                {
                    var payload = new
                    {
                        chat_id = user.ChatId,
                        text = messageBuilder.ToString(),
                        parse_mode = "HTML",
                        reply_markup = keyboard
                    };

                    var json = JsonSerializer.Serialize(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync(
                        $"https://api.telegram.org/bot{botToken}/sendMessage",
                        content
                    );

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError("Failed to send notification to @{Username} (ChatId: {ChatId}). Status: {StatusCode}, Error: {Error}",
                            user.Username, user.ChatId, response.StatusCode, errorContent);
                        failCount++;
                    }
                    else
                    {
                        _logger.LogInformation("Notification sent to @{Username} (ChatId: {ChatId})", user.Username, user.ChatId);
                        successCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending notification to @{Username} (ChatId: {ChatId})", user.Username, user.ChatId);
                    failCount++;
                }
            }

            _logger.LogInformation("Sale notification sent: {Success} successful, {Failed} failed", successCount, failCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Telegram notification for sale {SaleId}", sale.Id);
        }
    }
}
