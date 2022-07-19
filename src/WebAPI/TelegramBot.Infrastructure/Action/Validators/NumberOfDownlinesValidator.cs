using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Models;

namespace TelegramBot.Infrastructure.Action.Validators
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

            return Task.FromResult(Result.CreateErrorResult(messageBuilder.BuildInvalidNumerOfDownlineRange(account.Language, userResponse, maxValue)));
        }
    }
}