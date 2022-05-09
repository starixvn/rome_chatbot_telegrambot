using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Alpha.API.Client.Models.WinLoss;
using Alpha.API.Client.Services;
using Newtonsoft.Json;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Configurations;
using TelegramBot.Infrastructure.Shared.Models;
using TelegramBot.Infrastructure.Shared.Resources;
using TelegramBot.Infrastructure.Shared.Extensions;
using System.Text.RegularExpressions;

namespace TelegramBot.Infrastructure.Function.Handlers
{
  public class TodayWinLossFunctionHandler : BaseFunctionHandler, IFunctionHandler
  {
    private readonly IWinLossService winLossService;

    private readonly ICustomerService customerService;

    private readonly WebAPIConfiguration webAPIConfiguration;

    public TodayWinLossFunctionHandler(
        IMessageBuilder messageBuilder,
        IMessageSender messageSender,
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

    public async Task<Result> Handle(CustBotAccount account, string[] parameters)
    {
      var parameter = parameters[0];
      if (!string.IsNullOrWhiteSpace(parameter))
      {
        WinLossBase data = null;
        var viewTodayWinLoss = false;
        if (parameter == "m")
        {
          viewTodayWinLoss = true;
          data = await winLossService.GetWinLoss(account.CustId, account.CustomerLevel, DateTime.Now);
        }
        else
        {
          var downlineInfo = await customerService.GetDownlineInfo(account.CustId, parameter);
          if (!IsValidDirectDownline(account.CustomerLevel, downlineInfo))
          {
            return Result.CreateErrorResult(messageBuilder.BuildInvalidCustNameMessage(account.Language, parameter));
          }

          data = await winLossService.GetDownlineWinLoss(account.CustId, account.CustomerLevel, DateTime.Now, downlineInfo.CustId);
        }

        if (data == null)
        {
          return Result.CreateSuccessfulResult(messageBuilder.BuildNoDataMessage(account.Language));
        }

        var message = Result.CreateSuccessfulResult(
            viewTodayWinLoss
            ? BuildTodayWinLossMessage(account.Language, data, account.CustomerLevel)
            : BuildDownlineTodayWinLossMessage(account.Language, data, account.CustomerLevel, parameter));

        return message;
      }

      return Result.CreateErrorResult();
    }

    private string BuildTodayWinLossMessage(string language, WinLossBase todayWinLoss, byte custLevel)
    {
      var textContainer = new List<string>();
      AddTitleRow(textContainer, language);
      AddBodyMessage(textContainer, todayWinLoss);
      return string.Join(messageFormatter.NewLine(), textContainer);
    }

    private string BuildDownlineTodayWinLossMessage(string language, WinLossBase todayWinLoss, byte custLevel, string downlineUsername)
    {
      var textContainer = new List<string>();
      AddTitleRow(textContainer, language);
      textContainer.Add($"{SharedResource.Username}:");
      textContainer.Add($"    {downlineUsername}\n");
      AddBodyMessage(textContainer, todayWinLoss);
      return string.Join(messageFormatter.NewLine(), textContainer);
    }

    private void AddTitleRow(List<string> textContainer, string language)
        => textContainer.Add($"[{ResourceHelper.Get(nameof(SharedResource.TodayWinLoss), language)} - {DateTime.Now:yyyyMMdd}]");

    private void AddBodyMessage(List<string> textContainer, WinLossBase todayWinLoss)
    {
      var json = JsonConvert.SerializeObject(todayWinLoss);
      var dictionary = JsonConvert.DeserializeObject<Dictionary<string, decimal>>(json);
      foreach (var item in dictionary)
      {
        var key =  string.Join(" ", Regex.Split(item.Key, @"(?<!^)(?=[A-Z])"));
        textContainer.Add($"{key.ToUpper()}: {item.Value.ToFormattedNumber()}");
      }
    }

    public async Task<IEnumerable<string>> GetTopDownlineUsername(CustBotAccount account, int top = 5)
        => await winLossService.GetTopUsername(account.CustId, account.CustomerLevel, DateTime.Now, top);
  }
}