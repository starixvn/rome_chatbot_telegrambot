using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Alpha.API.Client.Models.WinLoss;
using Alpha.API.Client.Services;
using Newtonsoft.Json;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Configurations;
using TelegramBot.Infrastructure.Shared.Constants;
using TelegramBot.Infrastructure.Shared.Extensions;
using TelegramBot.Infrastructure.Shared.Models;
using TelegramBot.Infrastructure.Shared.Resources;

namespace TelegramBot.Infrastructure.Function.Handlers
{
    public abstract class WinLossFunctionHandler : BaseFunctionHandler
    {
        protected readonly IWinLossService winLossService;

        protected readonly ICustomerService customerService;

        protected readonly WebAPIConfiguration webAPIConfiguration;

        public WinLossFunctionHandler(
            IMessageBuilder messageBuilder,
            IMessageFormatter messageFormatter,
            IWinLossService winLossService,
            ICustomerService customerService,
            WebAPIConfiguration webAPIConfiguration)
            : base(messageBuilder, messageFormatter)
        {
            this.winLossService = winLossService;
            this.customerService = customerService;
            this.webAPIConfiguration = webAPIConfiguration;
        }

        public async Task<Result> HandleWinLoss(CustBotAccount account, string[] parameters, DateTime dateTime)
        {
            var parameter = parameters[0];
            if (!string.IsNullOrWhiteSpace(parameter))
            {
                WinLossBase data = null;
                var viewMyTodayWinLoss = false;
                if (parameter == "m")
                {
                    viewMyTodayWinLoss = true;
                    data = await winLossService.GetWinLoss(account.CustId, account.CustomerLevel, dateTime);
                }
                else
                {
                    var downlineInfo = await customerService.GetDownlineInfo(account.CustId, parameter);
                    if (!IsValidDirectDownline(account.CustomerLevel, downlineInfo))
                    {
                        return Result.CreateErrorResult(messageBuilder.BuildInvalidCustNameMessage(account.Language, parameter));
                    }

                    data = await winLossService.GetDownlineWinLoss(account.CustId, account.CustomerLevel, dateTime, downlineInfo.CustId);
                }

                if (data == null)
                {
                    return Result.CreateSuccessfulResult(messageBuilder.BuildNoDataMessage(account.Language));
                }

                var message = Result.CreateSuccessfulResult(
                    viewMyTodayWinLoss
                    ? BuildWinLossMessage(account.Language, data)
                    : BuildDownlineWinLossMessage(account.Language, data, parameter));

                return message;
            }

            return Result.CreateErrorResult();
        }

        protected string BuildWinLossMessage(string language, WinLossBase winLoss)
        {
            var textContainer = new List<string>();
            AddTitleRow(textContainer, language);
            AddBodyMessage(textContainer, winLoss, language);
            return string.Join(messageFormatter.NewLine(), textContainer);
        }

        protected string BuildDownlineWinLossMessage(string language, WinLossBase winLoss, string downlineUsername)
        {
            var textContainer = new List<string>();
            AddTitleRow(textContainer, language);
            textContainer.Add($"{ResourceHelper.Get(nameof(SharedResource.Username), language).ToUpperInvariant()}:");
            textContainer.Add($"    {downlineUsername}{messageFormatter.NewLine()}");
            AddBodyMessage(textContainer, winLoss, language);
            return string.Join(messageFormatter.NewLine(), textContainer);
        }

        protected abstract void AddTitleRow(List<string> textContainer, string language);

        protected void AddBodyMessage(List<string> textContainer, WinLossBase winLoss, string language)
        {
            foreach (var property in winLoss.GetType().GetProperties())
            {
                textContainer.Add($"{ResourceHelper.Get(property.Name, language).ToUpperInvariant()}: {property.GetValue(winLoss)}");
            }
        }
    }
}