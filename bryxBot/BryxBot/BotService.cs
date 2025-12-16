using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BryxBot;

public class BotService : BackgroundService
{
    private readonly ILogger<BotService> _logger;
    private readonly BotConfiguration _config;
    private readonly ITelegramBotClient _botClient;
    private readonly MessageHandler _messageHandler;

    public BotService(
        ILogger<BotService> logger,
        IOptions<BotConfiguration> config,
        MessageHandler messageHandler)
    {
        _logger = logger;
        _config = config.Value;
        _botClient = new TelegramBotClient(_config.BotToken);
        _messageHandler = messageHandler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Запуск Bryx Bot...");

        var me = await _botClient.GetMe(stoppingToken);
        _logger.LogInformation("Бот запущен: @{BotUsername}", me.Username);

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        await _botClient.ReceiveAsync(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            stoppingToken
        );
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            if (update.Message is { } message)
            {
                await _messageHandler.HandleMessage(botClient, message, cancellationToken);
            }
            else if (update.CallbackQuery is { } callbackQuery)
            {
                await _messageHandler.HandleCallbackQuery(botClient, callbackQuery, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке сообщения");
        }
    }

    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Ошибка Telegram Bot API");
        return Task.CompletedTask;
    }
}
