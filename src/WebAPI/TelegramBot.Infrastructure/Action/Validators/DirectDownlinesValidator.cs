using System.Threading.Tasks;
using Alpha.API.Client.Services;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Models;

namespace TelegramBot.Infrastructure.Action.Validators
{
    public class DirectDownlinesValidator : IActionValidator
    {
        private readonly IMessageBuilder messageBuilder;
        private readonly ICustomerService customerService;

        public DirectDownlinesValidator(IMessageBuilder messageBuilder, ICustomerService customerService)
        {
            this.messageBuilder = messageBuilder;
            this.customerService = customerService;
        }

        public async Task<Result> Validate(CustBotAccount account, string userResponse)
        {
            if (!string.IsNullOrEmpty(userResponse))
            {
                var downlineInfo = await customerService.GetDownlineInfo(account.CustId, userResponse);
                if (downlineInfo != null && downlineInfo.CustId > 0 && IsDirectDownline(account.CustomerLevel, downlineInfo.RoleId))
                {
                    return Result.CreateSuccessfulResult();
                }
            }

            return Result.CreateErrorResult(messageBuilder.BuildInvalidCustNameMessage(account.Language, userResponse));
        }

        private static bool IsDirectDownline(int custLevel, int downlineLevel)
            => (custLevel - downlineLevel) == 1;
    }
}