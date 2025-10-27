using Telegram.Bot;
using TelegramBot.Client.Services;
using TelegramBot.Receiver.Configurations;
using TelegramBot.Utilities;

namespace TelegramBot.Receiver.Services
{
    public class TelegramHostedService : IHostedService
    {
        private readonly Fanex.Logging.ILogger _logger;
        private readonly IHandleUpdateService _handleUpdateService;
        private readonly ReceiverConfiguration _receiverConfiguration;
        private readonly IBotService _botService;

        public TelegramHostedService(
            Fanex.Logging.ILogger logger,
            IHandleUpdateService handleUpdateService,
            ReceiverConfiguration receiverConfiguration,
            IBotService botService)
        {
            _logger = logger;
            _handleUpdateService = handleUpdateService;
            _receiverConfiguration = receiverConfiguration;
            _botService = botService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _logger.InfoAsync($"Worker {typeof(TelegramHostedService).Assembly.GetName().Name} running at: {DateTimeOffset.Now}");
                var botEntities = await _botService.GetAllAsync();
                var botClients = botEntities
                    .Select(botEntity => new
                    {
                        botEntity.AccessToken,
                        BotClient = TelegramBotUtils.InitBotClient(botEntity.AccessToken, _receiverConfiguration.ProxyAddress)
                    })
                    .ToDictionary(x => x.AccessToken, x => x.BotClient);

                _handleUpdateService.BotClients = botClients;
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
