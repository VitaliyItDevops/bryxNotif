using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;

namespace BryxBot;

public class MessageHandler
{
    private readonly ILogger<MessageHandler> _logger;
    private readonly BotConfiguration _config;
    private readonly HttpClient _httpClient;
    private List<string> _allowedUsers = new();
    private DateTime _lastUsersUpdate = DateTime.MinValue;
    private readonly TimeSpan _usersUpdateInterval = TimeSpan.FromMinutes(5);

    public MessageHandler(
        ILogger<MessageHandler> logger,
        IOptions<BotConfiguration> config,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _config = config.Value;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(_config.CrmApiUrl);

        // Загружаем список пользователей при старте
        _ = RefreshAllowedUsersAsync();
    }

    public async Task HandleMessage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        if (message.Text is not { } messageText)
            return;

        var chatId = message.Chat.Id;

        _logger.LogInformation("Получено сообщение от {ChatId}: {MessageText}", chatId, messageText);

        // Проверка авторизации
        if (!await IsUserAuthorizedAsync(chatId))
        {
            _logger.LogWarning("Неавторизованная попытка доступа от {ChatId}", chatId);
            await botClient.SendMessage(
                chatId: chatId,
                text: "⛔ Доступ запрещен. Этот бот предназначен только для авторизованных пользователей.",
                cancellationToken: cancellationToken
            );
            return;
        }

        var action = messageText.Split(' ')[0] switch
        {
            "/start" => HandleStart(botClient, chatId, cancellationToken),
            "/help" => HandleHelp(botClient, chatId, cancellationToken),
            "/menu" => HandleMenu(botClient, chatId, cancellationToken),
            "/products" => HandleProducts(botClient, chatId, cancellationToken),
            "/sales" => HandleSales(botClient, chatId, cancellationToken),
            "/stats" => HandleStats(botClient, chatId, cancellationToken),
            _ => HandleUnknown(botClient, chatId, cancellationToken)
        };

        await action;
    }

    private async Task HandleStart(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        var message = """
            👋 Добро пожаловать в Bryx CRM Bot!

            Я помогу вам управлять вашей CRM системой через Telegram.

            Используйте /help для просмотра доступных команд.
            Используйте /menu для доступа к главному меню.
            """;

        await botClient.SendMessage(
            chatId: chatId,
            text: message,
            cancellationToken: cancellationToken
        );
    }

    private async Task HandleHelp(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        var message = """
            📚 Доступные команды:

            /start - Приветственное сообщение
            /help - Список команд
            /menu - Главное меню
            /products - Просмотр товаров
            /sales - Просмотр продаж
            /stats - Статистика
            """;

        await botClient.SendMessage(
            chatId: chatId,
            text: message,
            cancellationToken: cancellationToken
        );
    }

    private async Task HandleMenu(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        var keyboard = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "📦 Товары", "💰 Продажи" },
            new KeyboardButton[] { "📊 Статистика", "ℹ️ Помощь" }
        })
        {
            ResizeKeyboard = true
        };

        await botClient.SendMessage(
            chatId: chatId,
            text: "Выберите раздел:",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken
        );
    }

    private async Task HandleProducts(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync("/products?pageSize=5");

            if (!response.IsSuccessStatusCode)
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "Не удалось получить данные о товарах. Проверьте, что CRM запущена.",
                    cancellationToken: cancellationToken
                );
                return;
            }

            var data = await response.Content.ReadFromJsonAsync<ProductsResponse>();

            if (data == null || data.Products.Count == 0)
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "📦 Товары не найдены",
                    cancellationToken: cancellationToken
                );
                return;
            }

            var message = $"📦 Товары (первые {data.Products.Count} из {data.Total}):\n\n";

            foreach (var product in data.Products)
            {
                var favorite = product.IsFavorite ? "⭐ " : "";
                var defective = product.IsDefective ? "⚠️ " : "";
                message += $"{favorite}{defective}{product.Name}\n";
                message += $"  └ Категория: {product.Category}\n";
                message += $"  └ Цена: {product.SalePrice:N2} грн\n";
                message += $"  └ Статус: {product.Status}\n\n";
            }

            if (data.Total > data.Products.Count)
            {
                message += $"Показано {data.Products.Count} из {data.Total} товаров";
            }

            await botClient.SendMessage(
                chatId: chatId,
                text: message,
                cancellationToken: cancellationToken
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении товаров");
            await botClient.SendMessage(
                chatId: chatId,
                text: "Произошла ошибка при получении данных. Убедитесь, что CRM запущена.",
                cancellationToken: cancellationToken
            );
        }
    }

    private async Task HandleSales(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync("/sales?pageSize=5");

            if (!response.IsSuccessStatusCode)
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "Не удалось получить данные о продажах. Проверьте, что CRM запущена.",
                    cancellationToken: cancellationToken
                );
                return;
            }

            var data = await response.Content.ReadFromJsonAsync<SalesResponse>();

            if (data == null || data.Sales.Count == 0)
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "💰 Продажи не найдены",
                    cancellationToken: cancellationToken
                );
                return;
            }

            var message = $"💰 Продажи (последние {data.Sales.Count} из {data.Total}):\n\n";

            foreach (var sale in data.Sales)
            {
                message += $"#{sale.Id} - {sale.Buyer}\n";
                message += $"  └ Дата: {sale.SaleDate:dd.MM.yyyy}\n";
                message += $"  └ Сумма: {sale.TotalAmount:N2} грн\n";
                message += $"  └ Товаров: {sale.ProductCount} шт.\n";
                message += $"  └ Статус: {sale.Status}\n\n";
            }

            if (data.Total > data.Sales.Count)
            {
                message += $"Показано {data.Sales.Count} из {data.Total} продаж";
            }

            await botClient.SendMessage(
                chatId: chatId,
                text: message,
                cancellationToken: cancellationToken
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении продаж");
            await botClient.SendMessage(
                chatId: chatId,
                text: "Произошла ошибка при получении данных. Убедитесь, что CRM запущена.",
                cancellationToken: cancellationToken
            );
        }
    }

    private async Task HandleStats(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync("/stats");

            if (!response.IsSuccessStatusCode)
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "Не удалось получить статистику. Проверьте, что CRM запущена.",
                    cancellationToken: cancellationToken
                );
                return;
            }

            var data = await response.Content.ReadFromJsonAsync<StatsResponse>();

            if (data == null)
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "Не удалось получить статистику",
                    cancellationToken: cancellationToken
                );
                return;
            }

            var message = "📊 Статистика Bryx CRM\n\n";

            message += "📦 Товары:\n";
            message += $"  └ Всего: {data.Products.Total}\n";
            message += $"  └ В наличии: {data.Products.InStock}\n";
            message += $"  └ Продано: {data.Products.Sold}\n";
            message += $"  └ Ожидается: {data.Products.Expected}\n\n";

            message += "💰 Продажи:\n";
            message += $"  └ Всего продаж: {data.Sales.Total}\n";
            message += $"  └ Общая сумма: {data.Sales.TotalAmount:N2} грн\n";
            message += $"  └ Сегодня продаж: {data.Sales.Today.Count}\n";
            message += $"  └ Сумма сегодня: {data.Sales.Today.Amount:N2} грн\n\n";

            if (data.Categories.Count > 0)
            {
                message += "📋 Топ категорий:\n";
                foreach (var category in data.Categories.Take(5))
                {
                    message += $"  └ {category.Category}: {category.Count} шт.\n";
                }
            }

            await botClient.SendMessage(
                chatId: chatId,
                text: message,
                cancellationToken: cancellationToken
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении статистики");
            await botClient.SendMessage(
                chatId: chatId,
                text: "Произошла ошибка при получении данных. Убедитесь, что CRM запущена.",
                cancellationToken: cancellationToken
            );
        }
    }

    private async Task HandleUnknown(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        await botClient.SendMessage(
            chatId: chatId,
            text: "Неизвестная команда. Используйте /help для просмотра доступных команд.",
            cancellationToken: cancellationToken
        );
    }

    public async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        if (callbackQuery.Data == null || callbackQuery.Message == null)
            return;

        var chatId = callbackQuery.Message.Chat.Id;

        _logger.LogInformation("Получен callback: {Data}", callbackQuery.Data);

        // Проверка авторизации
        if (!await IsUserAuthorizedAsync(chatId))
        {
            _logger.LogWarning("Неавторизованная попытка callback от {ChatId}", chatId);
            await botClient.AnswerCallbackQuery(
                callbackQuery.Id,
                "⛔ Доступ запрещен",
                showAlert: true,
                cancellationToken: cancellationToken
            );
            return;
        }

        try
        {
            // Обрабатываем callback кнопки "Отправлено"
            if (callbackQuery.Data.StartsWith("ship_"))
            {
                var saleIdString = callbackQuery.Data.Replace("ship_", "");
                if (int.TryParse(saleIdString, out int saleId))
                {
                    await HandleShipSale(botClient, callbackQuery, saleId, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке callback");
            await botClient.AnswerCallbackQuery(
                callbackQuery.Id,
                "Произошла ошибка. Попробуйте позже.",
                showAlert: true,
                cancellationToken: cancellationToken
            );
        }
    }

    private async Task HandleShipSale(ITelegramBotClient botClient, CallbackQuery callbackQuery, int saleId, CancellationToken cancellationToken)
    {
        try
        {
            // Отправляем запрос к CRM API для изменения статуса
            var response = await _httpClient.PostAsync($"sales/{saleId}/ship", null);

            if (response.IsSuccessStatusCode)
            {
                // Обновляем сообщение, убираем кнопку и добавляем статус
                var originalText = callbackQuery.Message.Text ?? "";
                var updatedText = originalText + "\n\n✅ <b>Статус: Отправлено</b>";

                await botClient.EditMessageText(
                    chatId: callbackQuery.Message.Chat.Id,
                    messageId: callbackQuery.Message.MessageId,
                    text: updatedText,
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken
                );

                // Отправляем уведомление пользователю
                await botClient.AnswerCallbackQuery(
                    callbackQuery.Id,
                    "✅ Продажа отмечена как отправленная!",
                    cancellationToken: cancellationToken
                );

                _logger.LogInformation("Sale {SaleId} marked as shipped successfully", saleId);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to mark sale as shipped. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);

                await botClient.AnswerCallbackQuery(
                    callbackQuery.Id,
                    "❌ Ошибка при обновлении статуса. Проверьте CRM.",
                    showAlert: true,
                    cancellationToken: cancellationToken
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking sale {SaleId} as shipped", saleId);
            await botClient.AnswerCallbackQuery(
                callbackQuery.Id,
                "❌ Ошибка при обновлении статуса.",
                showAlert: true,
                cancellationToken: cancellationToken
            );
        }
    }

    private async Task RefreshAllowedUsersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("users");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<AllowedUsersResponse>();
                if (data != null && data.AllowedUsers != null)
                {
                    _allowedUsers = data.AllowedUsers;
                    _lastUsersUpdate = DateTime.UtcNow;
                    _logger.LogInformation("Обновлён список разрешённых пользователей: {Count} пользователей", _allowedUsers.Count);
                }
            }
            else
            {
                _logger.LogWarning("Не удалось получить список пользователей из CRM. Статус: {StatusCode}", response.StatusCode);

                // Fallback на статическую конфигурацию
                if (_config.AllowedUsers != null && _config.AllowedUsers.Any())
                {
                    _allowedUsers = _config.AllowedUsers;
                    _logger.LogInformation("Используется статический список из конфигурации");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении списка пользователей");

            // Fallback на статическую конфигурацию
            if (_config.AllowedUsers != null && _config.AllowedUsers.Any())
            {
                _allowedUsers = _config.AllowedUsers;
            }
        }
    }

    private async Task<bool> IsUserAuthorizedAsync(long chatId)
    {
        // Обновляем список пользователей, если прошло достаточно времени
        if (DateTime.UtcNow - _lastUsersUpdate > _usersUpdateInterval)
        {
            await RefreshAllowedUsersAsync();
        }

        // Если список пуст, разрешаем всем (для обратной совместимости)
        if (_allowedUsers == null || !_allowedUsers.Any())
            return true;

        return _allowedUsers.Contains(chatId.ToString());
    }
}

public class AllowedUsersResponse
{
    public List<string> AllowedUsers { get; set; } = new();
    public int Count { get; set; }
}
