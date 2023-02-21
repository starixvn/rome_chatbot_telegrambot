using Alpha.API.Client.Models.Customers;
using Alpha.API.Client.Models.Outstanding;
using Alpha.API.Client.Services;
using TelegramBot.Infrastructure.Bot.Models;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Configurations;
using TelegramBot.Infrastructure.Shared.Extensions;
using TelegramBot.Infrastructure.Shared.Models;
using TelegramBot.Infrastructure.Shared.Resources;

namespace TelegramBot.Infrastructure.Function.Handlers
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

        public async Task<Result> Handle(CustBotAccount account, string[] parameters, BotEntity bot)
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

            var data = await outstandingService.GetDownlineOutstanding(account.CustId, account.CustomerLevel, downlineInfo.CustId, account.Language);
            var message = BuildDownlineOutstandingMessage(account.Language, data, account.CustomerLevel, username);
            return Result.CreateSuccessfulResult(message);
        }

        private async Task<Result> HandleTopOutstanding(CustBotAccount account, int topQuery)
        {
            if (topQuery <= 0 || topQuery > webAPIConfiguration.OutstandingMaxTopQuery)
            {
                return Result.CreateErrorResult(messageBuilder.BuildInvalidDownlinesMessage(account.Language, webAPIConfiguration.OutstandingMaxTopQuery));
            }

            var data = await outstandingService.GetTopOutstanding(account.CustId, account.CustomerLevel, topQuery);
            var message = BuildTopOutstandingMessage(account.Language, data, account.CustomerLevel);
            return Result.CreateSuccessfulResult(message);
        }

        private string BuildTopOutstandingMessage(string language, OutstandingReport data, byte custLevel)
        {
            var textContainer = new List<string>
            {
                $"[{ResourceHelper.Get(nameof(SharedResource.DownlineTopHighestOutstanding), language)}]"
            };
            AddBodyMessage(textContainer, data, custLevel, language);

            return string.Join(messageFormatter.NewLine(), textContainer);
        }

        private string BuildDownlineOutstandingMessage(string language, OutstandingReport data, byte custLevel, string downlineUsername)
        {
            var textContainer = new List<string>
            {
                $"[{ResourceHelper.Get(nameof(SharedResource.DownlineOutstanding), language)}]",
                $"{ResourceHelper.Get(nameof(SharedResource.For), language)} {downlineUsername}"
            };
            AddBodyMessage(textContainer, data, custLevel, language);

            return string.Join(messageFormatter.NewLine(), textContainer);
        }

        private void AddBodyMessage(List<string> textContainer, OutstandingReport data, byte custLevel, string language)
        {
            if (data == null || !data.Outstandings.Any())
            {
                textContainer.Add(messageBuilder.BuildNoDataMessage(language));
            }
            else
            {
                var downlineLevelText = ResourceHelper.Get($"CustLevel_{custLevel - 1}", language);
                var custLevelText = ResourceHelper.Get($"CustLevel_{custLevel}", language);
                var memberText = ResourceHelper.Get(nameof(SharedResource.Member), language);
                textContainer.Add("<pre>");
                textContainer.Add(messageFormatter.FormatHeaderRow(downlineLevelText, ResourceHelper.Get(nameof(SharedResource.Outstanding), language), language));
                textContainer.Add("________________________________");

                var index = 1;
                var length = data.Outstandings.Count();
                foreach (var outstanding in data.Outstandings)
                {
                    textContainer.Add(outstanding.Name);
                    textContainer.Add(messageFormatter.FormatRow(memberText, outstanding.MemberOutstanding.ToFormattedNumber(), language));
                    textContainer.Add(messageFormatter.FormatRow(custLevelText, outstanding.Outstanding.ToFormattedNumber(), language));
                    textContainer.Add(index == length ? "________________________________" : "--------------------------------");
                    index++;
                }
                textContainer.Add(ResourceHelper.Get(nameof(SharedResource.Total), language).ToUpperInvariant());
                textContainer.Add(messageFormatter.FormatRow(memberText, data.Total.MemberOutstanding.ToFormattedNumber(), language));
                textContainer.Add(messageFormatter.FormatRow(custLevelText, data.Total.Outstanding.ToFormattedNumber(), language));

                textContainer.Add("</pre>");
            }
        }

        public async Task<IEnumerable<string>> GetTopDownlineUsername(CustBotAccount account, int top = 5)
            => await outstandingService.GetTopUsername(account.CustId, account.CustomerLevel, top);
    }
}