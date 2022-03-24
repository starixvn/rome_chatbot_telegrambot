using Fanex.Logging;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types.Enums;
using TelegramBot.Client.Services;
using TelegramBot.Common.Configurations;

namespace TelegramBot.Receiver.Services
{
    public class TelegramHostedService : IHostedService
    {
        private readonly ILogger logger;
        private readonly IHandleUpdateService handleUpdateSerice;
        private readonly ReceiverConfiguration receiverConfiguration;
        private readonly IBotService botService;
        private readonly UpdateType[] allowedUpdates = new[]
        {
            UpdateType.Message,
            UpdateType.CallbackQuery
        };

        public TelegramHostedService(ILogger logger, IHandleUpdateService handleUpdateSerice, ReceiverConfiguration botConfiguration, IBotService botService)
        {
            this.logger = logger;
            this.handleUpdateSerice = handleUpdateSerice;
            this.receiverConfiguration = botConfiguration;
            this.botService = botService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await logger.InfoAsync($"Worker {typeof(TelegramHostedService).Assembly.GetName().Name} running at: {DateTimeOffset.Now}");
            handleUpdateSerice.BotEntities = await botService.GetAllAsync().ConfigureAwait(false);
            handleUpdateSerice.BotClients = handleUpdateSerice.BotEntities.Select(x => new TelegramBotClient(x.AccessToken));
            var tasks = handleUpdateSerice.BotClients.Select(botClient =>
            {
                if (receiverConfiguration.IsUsingWebhook)
                {
                    return botClient.SetWebhookAsync(
                        url: $"{receiverConfiguration.HostAddress}/{botClient.BotId}",
                        allowedUpdates: allowedUpdates,
                        cancellationToken: cancellationToken);
                }
                else
                {
                    botClient.StartReceiving(
                        handleUpdateSerice.HandleUpdateAsync,
                        handleUpdateSerice.HandleErrorAsync,
                        new ReceiverOptions { AllowedUpdates = allowedUpdates },
                        cancellationToken);
                    return botClient.GetMeAsync(cancellationToken);
                }
            });
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (receiverConfiguration.IsUsingWebhook)
            {
                var tasks = handleUpdateSerice.BotClients.Select(botClient => botClient.DeleteWebhookAsync(cancellationToken: cancellationToken));
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }

            await logger.InfoAsync($"Worker {typeof(TelegramHostedService).Assembly.GetName().Name} stopping at: {DateTimeOffset.Now}");
        }
    }
}
