using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types.Enums;
using TelegramBot.Client.Services;
using TelegramBot.Receiver.Configurations;

namespace TelegramBot.Receiver.Services
{
    public class TelegramHostedService : IHostedService
    {
        private readonly Fanex.Logging.ILogger _logger;
        private readonly IHandleUpdateService _handleUpdateSerice;
        private readonly ReceiverConfiguration _receiverConfiguration;
        private readonly IBotService _botService;
        private readonly UpdateType[] _allowedUpdates = new[]
        {
            UpdateType.Message,
            UpdateType.CallbackQuery
        };

        public TelegramHostedService(Fanex.Logging.ILogger logger, IHandleUpdateService handleUpdateSerice, ReceiverConfiguration receiverConfiguration, IBotService botService)
        {
            _logger = logger;
            _handleUpdateSerice = handleUpdateSerice;
            _receiverConfiguration = receiverConfiguration;
            _botService = botService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _logger.InfoAsync($"Worker {typeof(TelegramHostedService).Assembly.GetName().Name} running at: {DateTimeOffset.Now}");
            var botEntities = await _botService.GetAllAsync().ConfigureAwait(false);
            _handleUpdateSerice.BotEntities = botEntities.ToList();
            var botClients = new List<ITelegramBotClient>();
            var tasks = _handleUpdateSerice.BotEntities.Select(botEntity =>
            {
                var botClient = new TelegramBotClient(botEntity.AccessToken);
                botClients.Add(botClient);

                if (_receiverConfiguration.IsUsingWebhook)
                {
                    return botClient.SetWebhookAsync(
                        url: $"{_receiverConfiguration.HostAddress}/bot/{botEntity.AccessToken}",
                        allowedUpdates: _allowedUpdates,
                        cancellationToken: cancellationToken);
                }
                else
                {
                    botClient.StartReceiving(
                        _handleUpdateSerice.HandleUpdateAsync,
                        _handleUpdateSerice.HandleErrorAsync,
                        new ReceiverOptions { AllowedUpdates = _allowedUpdates },
                        cancellationToken);
                    return botClient.GetMeAsync(cancellationToken);
                }
            });
            _handleUpdateSerice.BotClients = botClients;
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_receiverConfiguration.IsUsingWebhook)
            {
                var tasks = _handleUpdateSerice.BotClients.Select(botClient => botClient.DeleteWebhookAsync(cancellationToken: cancellationToken));
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }

            await _logger.InfoAsync($"Worker {typeof(TelegramHostedService).Assembly.GetName().Name} stopping at: {DateTimeOffset.Now}");
        }
    }
}
