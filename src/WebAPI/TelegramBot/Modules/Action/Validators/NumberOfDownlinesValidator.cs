using TelegramBot.Modules.Message;
using TelegramBot.Modules.Setting.Models;
using TelegramBot.Shared.Models;

namespace TelegramBot.Modules.Action.Validators
{
    public class NumberOfDownlinesValidator : IActionValidator
    {
        private readonly int maxValue;
        private readonly IMessageBuilder messageBuilder;

        public NumberOfDownlinesValidator(IMessageBuilder messageBuilder, int maxValue)
        {
            this.messageBuilder = messageBuilder;
            this.maxValue = maxValue;
        }

        public Task<Result> Validate(CustBotAccount account, string userResponse)
        {
            if (!string.IsNullOrEmpty(userResponse) && int.TryParse(userResponse, out int number) && number > 0 && number <= maxValue)
            {
                return Task.FromResult(Result.CreateSuccessfulResult());
            }

            return Task.FromResult(Result.CreateErrorResult(messageBuilder.BuildInvalidNumberOfDownlineRange(account.Language, userResponse, maxValue)));
        }
    }
}