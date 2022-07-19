using Telegram.Bot;
using TelegramBot.Client.Services;
using TelegramBot.Receiver.Configurations;
using TelegramBot.Utilities;

namespace TelegramBot.Receiver.Services
{
    public class TelegramHostedService : IHostedService
    {
        private readonly Fanex.Logging.ILogger _logger;
        private readonly IHandleUpdateService _handleUpdateSerice;
        private readonly ReceiverConfiguration _receiverConfiguration;
        private readonly IBotService _botService;

        public TelegramHostedService(Fanex.Logging.ILogger logger, IHandleUpdateService handleUpdateSerice, ReceiverConfiguration receiverConfiguration, IBotService botService)
        {
            _logger = logger;
            _handleUpdateSerice = handleUpdateSerice;
            _receiverConfiguration = receiverConfiguration;
            _botService = botService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _logger.InfoAsync($"Worker {typeof(TelegramHostedService).Assembly.GetName().Name} running at: {DateTimeOffset.Now}");
                var botEntities = await _botService.GetAllAsync();
                var botClients = new Dictionary<string, ITelegramBotClient>();

                foreach (var botEntity in botEntities)
                {
                    var botClient = TelegramBotUtils.InitBotClient(botEntity.AccessToken, _receiverConfiguration.ProxyAddress);
                    botClients.Add(botEntity.AccessToken, botClient);
                }

                _handleUpdateSerice.BotClients = botClients;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _logger.InfoAsync($"Worker {typeof(TelegramHostedService).Assembly.GetName().Name} stopping at: {DateTimeOffset.Now}");
        }
    }
}
