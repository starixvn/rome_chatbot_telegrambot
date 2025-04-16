using Alpha.API.Client.Models.Customers;
using Alpha.API.Client.Models.Outstanding;
using Alpha.API.Client.Services;
using TelegramBot.Modules.Message;
using TelegramBot.Modules.Setting.Models;
using TelegramBot.Shared.Configurations;
using TelegramBot.Shared.Extensions;
using TelegramBot.Shared.Models;
using TelegramBot.Shared.Resources;

namespace TelegramBot.Modules.Function.Handlers
{
    public class OutstandingFunctionHandler : BaseFunctionHandler, IFunctionHandler
    {
        private readonly IOutstandingService outstandingService;
        private readonly ICustomerService customerService;
        private readonly WebAPIConfiguration webAPIConfiguration;

        public OutstandingFunctionHandler(
            IMessageBuilder messageBuilder,
            IMessageFormatter messageFormatter,
            IOutstandingService outstandingService,
            ICustomerService customerService,
            WebAPIConfiguration webAPIConfiguration)
            : base(messageBuilder, messageFormatter)
        {
            this.outstandingService = outstandingService;
            this.customerService = customerService;
            this.webAPIConfiguration = webAPIConfiguration;
        }

        public async Task<Result> Handle(CustBotAccount account, string[] parameters)
        {
            var parameter = parameters[0]?.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(parameter))
            {
                return Result.CreateErrorResult();
            }

            var downlineInfo = await customerService.GetDownlineInfo(account.CustId, parameter);
            if (downlineInfo != null && downlineInfo.CustId > 0)
            {
                return await HandleDownlineOutstanding(account, downlineInfo, parameter);
            }

            return int.TryParse(parameter, out int topQuery)
                ? await HandleTopOutstanding(account, topQuery)
                : Result.CreateErrorResult(messageBuilder.BuildInvalidUsernameMessage(account.Language));
        }

        private async Task<Result> HandleDownlineOutstanding(CustBotAccount account, DownlineInfo downlineInfo, string username)
        {
            if (!IsValidDirectDownline(account.CustomerLevel, downlineInfo))
            {
                return Result.CreateErrorResult(messageBuilder.BuildInvalidUsernameMessage(account.Language));
            }

            var data = await outstandingService.GetDownlineOutstanding(account.CustId, account.CustomerLevel, (byte)(downlineInfo.RoleId + 1), downlineInfo.CustId, account.System, account.Language);
            var message = BuildDownlineOutstandingMessage(account.Language, data, account, username, downlineInfo);
            return Result.CreateSuccessfulResult(message);
        }

        private async Task<Result> HandleTopOutstanding(CustBotAccount account, int topQuery)
        {
            if (topQuery <= 0 || topQuery > webAPIConfiguration.OutstandingMaxTopQuery)
            {
                return Result.CreateErrorResult(messageBuilder.BuildInvalidDownlinesMessage(account.Language, webAPIConfiguration.OutstandingMaxTopQuery));
            }

            var data = await outstandingService.GetTopOutstanding(account.CustId, account.CustomerLevel, topQuery);
            var message = BuildTopOutstandingMessage(account.Language, data, account);
            return Result.CreateSuccessfulResult(message);
        }

        private string BuildTopOutstandingMessage(string language, OutstandingReport data, CustBotAccount account)
        {
            var textContainer = new List<string>
            {
                $"[{ResourceHelper.Get(nameof(SharedResource.DownlineTopHighestOutstanding), language)}]"
            };
            AddTopOutstandingBodyMessage(textContainer, data, account, language);

            return string.Join(messageFormatter.NewLine(), textContainer);
        }

        private string BuildDownlineOutstandingMessage(string language, OutstandingReport data, CustBotAccount account, string downlineUsername, DownlineInfo downlineInfo)
        {
            var textContainer = new List<string>
            {
                $"[{ResourceHelper.Get(nameof(SharedResource.DownlineOutstanding), language)}]",
                $"{ResourceHelper.Get(nameof(SharedResource.For), language)} {downlineUsername}"
            };
            AddDownlineOutstandingBodyMessage(textContainer, data, account, language, downlineInfo);

            return string.Join(messageFormatter.NewLine(), textContainer);
        }

        private void AddTopOutstandingBodyMessage(List<string> textContainer, OutstandingReport data, CustBotAccount account, string language)
        {
            if (data == null || !data.Outstandings.Any())
            {
                textContainer.Add(messageBuilder.BuildNoDataMessage(language));
                return;
            }

            textContainer.Add("<pre>");
            textContainer.Add(messageFormatter.FormatHeaderRow(ResourceHelper.Get(nameof(SharedResource.Username), language), ResourceHelper.Get(nameof(SharedResource.Outstanding), language), language));
            textContainer.Add(messageFormatter.FormatHeaderRow("", ResourceHelper.Get(nameof(SharedResource.SportsbookRiskAmount), language), language));
            textContainer.Add(string.Join("", Enumerable.Repeat("_", TelegramFormatter.MaxRowLength + 1)));

            var custLevelText = ResourceHelper.Get($"CustLevel_{account.CustomerLevel}", language);
            var memberText = ResourceHelper.Get(nameof(SharedResource.Member), language);
            var downlineLevelText = ResourceHelper.Get($"CustLevelFull_{account.CustomerLevel - 1}", language);
            var index = 1;
            var length = data.Outstandings.Count();
            foreach (var outstanding in data.Outstandings)
            {
                var level = outstanding.IsDirectMember ? ResourceHelper.Get(nameof(SharedResource.DirectMember), language) : downlineLevelText;
                textContainer.Add(messageFormatter.FormatRow(outstanding.Name, level, language));
                textContainer.Add(messageFormatter.FormatRow(memberText, outstanding.MemberOutstanding.ToFormattedNumber(), language));
                textContainer.Add(messageFormatter.FormatRow("", outstanding.ToWin.ToFormattedNumber(), language));

                textContainer.Add(string.Join("", Enumerable.Repeat("-", 25)));
                textContainer.Add(messageFormatter.FormatRow(custLevelText, outstanding.Outstanding.ToFormattedNumber(), language));
                textContainer.Add(messageFormatter.FormatRow("", outstanding.RiskAmount.ToFormattedNumber(), language));
                var paddingChar = index == length ? "_" : "-";
                textContainer.Add(string.Join("", Enumerable.Repeat(paddingChar, TelegramFormatter.MaxRowLength + 1)));
                index++;
            }

            textContainer.Add(ResourceHelper.Get(nameof(SharedResource.Total), language).ToUpperInvariant());
            var toWin = TotalHasToWinValue(data.Outstandings) ? data.Total.ToWin.ToFormattedNumber() : NumberExtensions.NotAvaibleNumber;
            var riskAmout = TotalHasToWinValue(data.Outstandings) ? data.Total.RiskAmount.ToFormattedNumber() : NumberExtensions.NotAvaibleNumber;

            
            textContainer.Add(messageFormatter.FormatRow(memberText, data.Total.MemberOutstanding.ToFormattedNumber(), language));
            textContainer.Add(messageFormatter.FormatRow("", toWin, language));
            textContainer.Add(messageFormatter.FormatRow(custLevelText, data.Total.Outstanding.ToFormattedNumber(), language));
            textContainer.Add(messageFormatter.FormatRow("", riskAmout, language));

            textContainer.Add("</pre>");
        }

        private void AddDownlineOutstandingBodyMessage(List<string> textContainer, OutstandingReport data, CustBotAccount account, string language, DownlineInfo downlineInfo)
        {
            if (data == null || !data.Outstandings.Any())
            {
                textContainer.Add(messageBuilder.BuildNoDataMessage(language));
                return;
            }

            var downlineLevelText = account.CustomerLevel != 2 && downlineInfo.RoleId == 1
                ? ResourceHelper.Get(nameof(SharedResource.DirectMember), language)
                : ResourceHelper.Get($"CustLevelFull_{account.CustomerLevel - 1}", language);
            textContainer.Add("<pre>");
            textContainer.Add(messageFormatter.FormatHeaderRow(downlineLevelText, ResourceHelper.Get(nameof(SharedResource.Outstanding), language), language));
            textContainer.Add(messageFormatter.FormatHeaderRow("", ResourceHelper.Get(nameof(SharedResource.SportsbookRiskAmount), language), language));
            textContainer.Add(string.Join("", Enumerable.Repeat("_", TelegramFormatter.MaxRowLength + 1)));

            var custLevelText = ResourceHelper.Get($"CustLevel_{account.CustomerLevel}", language);
            var memberText = ResourceHelper.Get(nameof(SharedResource.Member), language);
            var index = 1;
            var length = data.Outstandings.Count();
            foreach (var outstanding in data.Outstandings)
            {
                textContainer.Add(outstanding.Name);
                textContainer.Add(messageFormatter.FormatRow(memberText, outstanding.MemberOutstanding.ToFormattedNumber(), language));
                textContainer.Add(messageFormatter.FormatRow("", outstanding.ToWin.ToFormattedNumber(), language));
                textContainer.Add(string.Join("", Enumerable.Repeat("-", 25)));
                textContainer.Add(messageFormatter.FormatRow(custLevelText, outstanding.Outstanding.ToFormattedNumber(), language));
                textContainer.Add(messageFormatter.FormatRow("", outstanding.RiskAmount.ToFormattedNumber(), language));
                var paddingChar = index == length ? "_" : "-";
                textContainer.Add(string.Join("", Enumerable.Repeat(paddingChar, TelegramFormatter.MaxRowLength + 1)));
                index++;
            }
            textContainer.Add(ResourceHelper.Get(nameof(SharedResource.Total), language).ToUpperInvariant());
            var toWin = TotalHasToWinValue(data.Outstandings) ? data.Total.ToWin.ToFormattedNumber() : NumberExtensions.NotAvaibleNumber;
            var riskAmount = TotalHasToWinValue(data.Outstandings) ? data.Total.RiskAmount.ToFormattedNumber() : NumberExtensions.NotAvaibleNumber;
            
            textContainer.Add(messageFormatter.FormatRow(memberText, data.Total.MemberOutstanding.ToFormattedNumber(), language));
            textContainer.Add(messageFormatter.FormatRow("", toWin, language));
            textContainer.Add(messageFormatter.FormatRow(custLevelText, data.Total.Outstanding.ToFormattedNumber(), language));
            textContainer.Add(messageFormatter.FormatRow("", riskAmount, language));

            textContainer.Add("</pre>");
        }

        public async Task<IEnumerable<string>> GetTopDownlineUsername(CustBotAccount account, int top = 5)
            => await outstandingService.GetTopUsername(account.CustId, account.CustomerLevel, top);

        private static bool TotalHasToWinValue(IEnumerable<OutstandingEntity> data)
            => data.Any(i => i.ToWin.HasValue);
    }
}