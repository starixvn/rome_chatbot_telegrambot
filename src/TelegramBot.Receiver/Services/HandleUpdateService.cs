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
using TelegramBot.Common.Constants;
using TelegramBot.Common.Entities;

namespace TelegramBot.Receiver.Services
{
    public interface IHandleUpdateService
    {
        List<BotEntity> BotEntities { get; set; }
        List<ITelegramBotClient> BotClients { get; set; }
        Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken = default);
        Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken);
    }

    public class HandleUpdateService : IHandleUpdateService
    {
        private readonly ILogger _logger;
        private readonly ReceiverConfiguration _botConfiguration;
        private readonly ICommandService _commandService;
        private readonly ISettingService _settingService;

        public List<BotEntity> BotEntities { get; set; }
        public List<ITelegramBotClient> BotClients { get; set; }

        public HandleUpdateService(ILogger logger, ReceiverConfiguration botConfiguration, ICommandService commandService, ISettingService settingService)
        {
            _logger = logger;
            _botConfiguration = botConfiguration;
            _commandService = commandService;
            _settingService = settingService;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken = default)
        {
            if (_botConfiguration.IsMaintenance)
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
                UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, update.CallbackQuery!),
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

        private async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            if (message.Type != MessageType.Text)
            {
                return;
            }

            if (message.Text!.StartsWith("/start"))
            {
                await _settingService.StartAsync(botClient.BotId.Value, message.Chat.Id, message.Text, message.From.Username, LanguageCodes.English);
            }
            else
            {
                await _commandService.ExecuteAsync(botClient.BotId.Value, message.Chat.Id, message.Text, false);
            }
        }

        private async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            await _commandService.ExecuteAsync(botClient.BotId.Value, callbackQuery.Message.Chat.Id, callbackQuery.Data, true);
        }

        private async Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
        {
            await _logger.ErrorAsync($"Unknown update: {update.Type}").ConfigureAwait(false);
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
            var errorMessage = exception is ApiRequestException apiRequestException
                ? $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}"
                : exception.ToString();

            await _logger.ErrorAsync(errorMessage);
        }
    }
}
