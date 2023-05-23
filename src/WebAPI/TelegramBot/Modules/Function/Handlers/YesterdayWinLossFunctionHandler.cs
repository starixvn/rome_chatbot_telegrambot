using Alpha.API.Client.Services;
using TelegramBot.Modules.Message;
using TelegramBot.Modules.Setting.Models;
using TelegramBot.Shared.Models;
using TelegramBot.Shared.Resources;
using TelegramBot.Utilities.Extensions;
using Alpha.API.Client.Models.WinLoss;

namespace TelegramBot.Modules.Function.Handlers
{
    public class YesterdayWinLossFunctionHandler : BaseWinLossFunctionHandler, IFunctionHandler
    {
        public YesterdayWinLossFunctionHandler(
            IMessageBuilder messageBuilder,
            IMessageFormatter messageFormatter,
            IWinLossService winLossService,
            ICustomerService customerService)
            : base(messageBuilder, messageFormatter, winLossService, customerService)
        {
        }

        public async Task<Result> Handle(CustBotAccount account, string[] parameters)
        {
            var parameter = parameters[0]?.ToLowerInvariant();
            if (string.IsNullOrEmpty(parameter))
            {
                return Result.CreateErrorResult();
            }

            if (parameter == "ne")
            {
                var enteredResult = await winLossService.GetYesterdayEnteredResult();
                return Result.CreateSuccessfulResult(BuildYesterdayNonEnteredMatchesMessage(account.Language, enteredResult));
            }
            else
            {
                return await HandleWinLoss(account, parameters, DateTime.Today.AddDays(-1));
            }
        }

        protected override void AddMyWinLossTitleRow(List<string> textContainer, string language)
            => textContainer.Add($"[{ResourceHelper.Get(nameof(SharedResource.MyYesterdayWinLoss), language)} - {DateTime.Today.AddDays(-1).ToSimpleDate()}]");

        public async Task<IEnumerable<string>> GetTopDownlineUsername(CustBotAccount account, int top = 5)
            => await winLossService.GetTopUsername(account.CustId, account.CustomerLevel, DateTime.Today.AddDays(-1), top);

        protected override void AddDownlineWinLossTitleRow(List<string> textContainer, string language)
            => textContainer.Add($"[{ResourceHelper.Get(nameof(SharedResource.DownlineYesterdayWinLoss), language)} - {DateTime.Today.AddDays(-1).ToSimpleDate()}]");

        private string BuildYesterdayNonEnteredMatchesMessage(string language, EnteredResult enteredResult)
        {
            var textContainer = new List<string>
            {
                $"[{ResourceHelper.Get(nameof(SharedResource.YesterdayNonEnteredMatches), language)} - {DateTime.Today.AddDays(-1).ToSimpleDate()}]",
                "<pre>",
                messageFormatter.FormatRow(ResourceHelper.Get(nameof(SharedResource.NonEnteredMatches), language), enteredResult.NonEntered.ToString(), language),
                messageFormatter.FormatRow(ResourceHelper.Get(nameof(SharedResource.EnteredMatches), language), enteredResult.Entered.ToString(), language),
                "</pre>"
            };

            return string.Join(messageFormatter.NewLine(), textContainer);
        }
    }
}