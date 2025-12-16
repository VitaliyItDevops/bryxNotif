using bryx_CRM.Data.Models;
using System.Text;
using System.Text.Json;

namespace bryx_CRM.Services;

public class TelegramNotificationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TelegramNotificationService> _logger;
    private readonly IConfiguration _configuration;

    public TelegramNotificationService(
        IHttpClientFactory httpClientFactory,
        ILogger<TelegramNotificationService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendSaleNotification(Sale sale, List<Product> products)
    {
        try
        {
            var botToken = _configuration["TelegramBot:BotToken"];
            var chatId = _configuration["TelegramBot:ChatId"];

            if (string.IsNullOrEmpty(botToken) || string.IsNullOrEmpty(chatId))
            {
                _logger.LogWarning("Telegram bot configuration is missing. Skipping notification.");
                return;
            }

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

            // Отправляем сообщение
            var payload = new
            {
                chat_id = chatId,
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
                _logger.LogError("Failed to send Telegram notification. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);
            }
            else
            {
                _logger.LogInformation("Sale notification sent successfully to Telegram chat {ChatId}", chatId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Telegram notification for sale {SaleId}", sale.Id);
        }
    }
}
