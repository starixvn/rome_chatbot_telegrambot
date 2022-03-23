using Fanex.Logging;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types.Enums;
using TelegramBot.Client.Services;
using TelegramBot.Common.Configurations;

namespace TelegramBot.Receiver.Services
{
    public class WebhookService : IHostedService
    {
        private readonly ILogger logger;
        private readonly IHandleUpdateService handleUpdateSerice;
        private readonly BotConfiguration botConfiguration;
        private readonly IBotService botService;
        private readonly UpdateType[] allowedUpdates = new UpdateType[]
        {
            UpdateType.Message,
            UpdateType.CallbackQuery
        };

        public WebhookService(ILogger logger, IHandleUpdateService handleUpdateSerice, BotConfiguration botConfiguration, IBotService botService)
        {
            this.logger = logger;
            this.handleUpdateSerice = handleUpdateSerice;
            this.botConfiguration = botConfiguration;
            this.botService = botService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await logger.InfoAsync($"Worker {Assembly.GetExecutingAssembly().GetName().Name} running at: {DateTimeOffset.Now}");
            handleUpdateSerice.BotEntities = await botService.GetAllAsync().ConfigureAwait(false);
            handleUpdateSerice.BotClients = handleUpdateSerice.BotEntities.Select(x => new TelegramBotClient(x.AccessToken));
            var tasks = handleUpdateSerice.BotClients.Select(botClient =>
            {
                if (string.IsNullOrEmpty(botConfiguration.HostAddress))
                {
                    botClient.StartReceiving(
                        handleUpdateSerice.HandleUpdateAsync,
                        handleUpdateSerice.HandleErrorAsync,
                        new ReceiverOptions { AllowedUpdates = allowedUpdates },
                        cancellationToken);
                    return botClient.GetMeAsync(cancellationToken);
                }
                else
                {
                    return botClient.SetWebhookAsync(
                        url: $"{botConfiguration.HostAddress}/{botClient.BotId}",
                        allowedUpdates: allowedUpdates,
                        cancellationToken: cancellationToken);
                }
            });
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(botConfiguration.HostAddress))
            {
                var tasks = handleUpdateSerice.BotClients.Select(botClient => botClient.DeleteWebhookAsync(cancellationToken: cancellationToken));
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }

            await logger.InfoAsync($"Worker {Assembly.GetExecutingAssembly().GetName().Name} stopping at: {DateTimeOffset.Now}");
        }
    }
}
