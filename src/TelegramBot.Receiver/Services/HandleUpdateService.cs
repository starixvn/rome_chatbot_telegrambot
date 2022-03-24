using Fanex.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Client.Services;
using TelegramBot.Common.Configurations;
using TelegramBot.Common.Entities;

namespace TelegramBot.Receiver.Services
{
    public interface IHandleUpdateService
    {
        IEnumerable<BotEntity> BotEntities { get; set; }
        IEnumerable<ITelegramBotClient> BotClients { get; set; }
        Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken = default);
        Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken);
    }

    public class HandleUpdateService : IHandleUpdateService
    {
        private readonly ILogger logger;
        private readonly ReceiverConfiguration botConfiguration;
        private readonly ICommandService commandService;

        public IEnumerable<BotEntity> BotEntities { get; set; }
        public IEnumerable<ITelegramBotClient> BotClients { get; set; }

        public HandleUpdateService(ILogger logger, ReceiverConfiguration botConfiguration, ICommandService commandService)
        {
            this.logger = logger;
            this.botConfiguration = botConfiguration;
            this.commandService = commandService;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken = default)
        {
            if (botConfiguration.IsMaintenance)
            {
                try
                {
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: "Bot is under maintenance!",
                        cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    await HandleErrorAsync(exception).ConfigureAwait(false);
                }

                return;
            }

            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(update.Message!),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(update.CallbackQuery!),
                _ => UnknownUpdateHandlerAsync(botClient, update)
            };
            try
            {
                await handler.ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(exception).ConfigureAwait(false);
            }
        }

        private Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            return BotOnMessageReceived(message);
        }

        private async Task BotOnMessageReceived(Message message)
        {
            if (message.Type != MessageType.Text)
                return;

            await commandService.ExecuteAsync(message.Chat.Id, message.Text, false);
        }

        private Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            return BotOnCallbackQueryReceived(callbackQuery);
        }

        private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
        {
            await commandService.ExecuteAsync(callbackQuery.Message.Chat.Id, callbackQuery.Data, true);
        }

        private async Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
        {
            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: "Unknown update!").ConfigureAwait(false);
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            return HandleErrorAsync(exception);
        }

        public async Task HandleErrorAsync(Exception exception)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            await logger.ErrorAsync(errorMessage);
        }
    }
}
