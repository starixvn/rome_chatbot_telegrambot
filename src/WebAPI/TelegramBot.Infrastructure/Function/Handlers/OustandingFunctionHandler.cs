using Alpha.API.Client.Exceptions;
using Alpha.API.Client.Models.Customers;
using Alpha.API.Client.Models.Outstanding;
using Alpha.API.Client.Services;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Configurations;
using TelegramBot.Infrastructure.Shared.Extensions;
using TelegramBot.Infrastructure.Shared.Models;
using TelegramBot.Infrastructure.Shared.Resources;

namespace TelegramBot.Infrastructure.Function.Handlers
{
    public class OustandingFunctionHandler : BaseFunctionHandler, IFunctionHandler
    {
        private readonly IOutstandingService outstandingService;
        private readonly ICustomerService customerService;
        private readonly WebAPIConfiguration webAPIConfiguration;

        public OustandingFunctionHandler(
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

            string message;
            try
            {
                var data = await outstandingService.GetDownlineOutstanding(account.CustId, account.CustomerLevel, downlineInfo.CustId, account.Language);
                message = BuildDownlineOutstandingMessage(account.Language, data, account.CustomerLevel, username);
            }
            catch (MinimizeUMException ex)
            {
                message = string.Format(ResourceHelper.Get(nameof(SharedResource.ApiException)), ex.StartTime, ex.EndTime);
            }

            return Result.CreateSuccessfulResult(message);
        }

        private async Task<Result> HandleTopOutstanding(CustBotAccount account, int topQuery)
        {
            if (topQuery <= 0 || topQuery > webAPIConfiguration.OutstandingMaxTopQuery)
            {
                return Result.CreateErrorResult(messageBuilder.BuildInvalidDownlinesMessage(account.Language, webAPIConfiguration.OutstandingMaxTopQuery));
            }

            string message;
            try
            {
                var data = await outstandingService.GetTopOutstanding(account.CustId, account.CustomerLevel, topQuery);
                message = BuildTopOutstandingMessage(account.Language, data, account.CustomerLevel);
            }
            catch (MinimizeUMException ex)
            {
                message = string.Format(ResourceHelper.Get(nameof(SharedResource.ApiException)), ex.StartTime, ex.EndTime);
            }

            return Result.CreateSuccessfulResult(message);
        }

        private string BuildTopOutstandingMessage(string language, OutstandingReport data, byte custLevel)
        {
            var headerRow = string.Format(
                "{0} / {1} {2} {3}",
                ResourceHelper.Get($"CustLevel_{custLevel - 1}", language),
                ResourceHelper.Get(nameof(SharedResource.Member), language),
                messageFormatter.Point(),
                ResourceHelper.Get($"CustLevel_{custLevel}", language));

            var textContainer = new List<string>();
            AddTitleRow(textContainer, language);
            textContainer.Add($"{headerRow}{messageFormatter.NewLine()}");

            if (data == null || !data.Outstandings.Any())
            {
                textContainer.Add(messageBuilder.BuildNoDataMessage(language));
            }
            else
            {
                AddTotalRow(textContainer, data.Total, language);
                AddBodyMessage(textContainer, data.Outstandings);
            }

            return string.Join(messageFormatter.NewLine(), textContainer);
        }

        private string BuildDownlineOutstandingMessage(string language, OutstandingReport data, byte custLevel, string downlineUsername)
        {
            var headerRow = string.Format(
                "{0} / {1} {2} {3}",
                ResourceHelper.Get(nameof(SharedResource.Products), language),
                ResourceHelper.Get(nameof(SharedResource.Member), language),
                messageFormatter.Point(),
                ResourceHelper.Get($"CustLevel_{custLevel}", language));

            var textContainer = new List<string>();
            AddTitleRow(textContainer, language);
            textContainer.Add($"{ResourceHelper.Get(nameof(SharedResource.For), language)} {downlineUsername}");
            textContainer.Add($"{headerRow}{messageFormatter.NewLine()}");

            if (data == null || !data.Outstandings.Any())
            {
                textContainer.Add(messageBuilder.BuildNoDataMessage(language));
            }
            else
            {
                AddTotalRow(textContainer, data.Total, language);
                AddBodyMessage(textContainer, data.Outstandings);
            }

            return string.Join(messageFormatter.NewLine(), textContainer);
        }

        private static void AddTitleRow(List<string> textContainer, string language)
            => textContainer.Add($"[{ResourceHelper.Get(nameof(SharedResource.Outstanding), language)}]");

        private void AddTotalRow(List<string> textContainer, OutstandingEntity total, string language)
        {
            textContainer.Add(messageFormatter.Bold(ResourceHelper.Get(nameof(SharedResource.Total), language).ToUpperInvariant()));
            textContainer.Add($"    {total.MemberOutstanding.ToFormattedNumber()} {messageFormatter.Point()} {total.Outstanding.ToFormattedNumber()}");
        }

        private void AddBodyMessage(List<string> textContainer, IEnumerable<OutstandingEntity> outstandings)
        {
            var index = 1;
            foreach (var outstanding in outstandings)
            {
                textContainer.Add(messageFormatter.Bold($"{index}. {outstanding.Name}"));
                textContainer.Add($"    {outstanding.MemberOutstanding.ToFormattedNumber()} {messageFormatter.Point()} {outstanding.Outstanding.ToFormattedNumber()}");
                index++;
            }
        }

        public async Task<IEnumerable<string>> GetTopDownlineUsername(CustBotAccount account, int top = 5)
            => await outstandingService.GetTopUsername(account.CustId, account.CustomerLevel, top);
    }
}