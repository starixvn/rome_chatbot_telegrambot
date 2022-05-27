using System.Text;
using Alpha.API.Client.Models.WinLoss;
using Fanex.Data.Repository;
using TelegramBot.Importer.Constants;
using TelegramBot.Importer.Criteria.YesterdayWinLoss;
using TelegramBot.Importer.Extensions;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Models.PersonalMessages;
using TelegramBot.Importer.Resources;
using ClientServices = Alpha.API.Client.Services; 

namespace TelegramBot.Importer.Services;

public interface IYesterdayWinLossService
{
    Task<WinLossBase> GetYesterdayWinLoss(int custId, byte custLevel, DateTime winLossDate);

    string BuildMessage(WinLossBase winloss, byte custLevel, string language, string winLossDate);

    Task<Result> Import(
        IEnumerable<ImportYesterdayWinLoss> notifications,
        DateTime winLossDate,
        string environment,
        int maxUserId);
}

public class YesterdayWinLossService : IYesterdayWinLossService
{
    private readonly ClientServices.IWinLossService winLossService;
    private readonly IDynamicRepository dynamicRepository;

    public YesterdayWinLossService(
        ClientServices.IWinLossService winLossService,
        IDynamicRepository dynamicRepository)
    {
        this.winLossService = winLossService;
        this.dynamicRepository = dynamicRepository;
    }

    public Task<WinLossBase> GetYesterdayWinLoss(int custId, byte custLevel, DateTime winLossDate)
        => winLossService.GetWinLoss(custId, custLevel, winLossDate);

    public string BuildMessage(WinLossBase winloss, byte custLevel, string language, string winLossDate)
    {
        var messageBuilder = new StringBuilder();
        AddTitle(messageBuilder, language, winLossDate);

        if (winloss == null)
        {
            messageBuilder.Append(TemplateHelper.Get("NoInfo", language));
        }
        else
        {
            switch (custLevel)
            {
                case CustomerLevel.Super:
                    AddContent(messageBuilder, (WinLossMaster)winloss, language);
                    break;

                case CustomerLevel.Master:
                    AddContent(messageBuilder, (WinLossAgent)winloss, language);
                    break;

                case CustomerLevel.Agent:
                    AddContent(messageBuilder, (WinLossMember)winloss, language);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(custLevel),
                        $"Not expected {nameof(custLevel)} value: {custLevel}");
            }
        }

        return System.Security.SecurityElement.Escape(messageBuilder.ToString());
    }

    private static void AddTitle(StringBuilder messageBuilder, string language, string winLossDate)
        => messageBuilder.AppendLine($"[{TemplateHelper.Get("YesterdayWinLoss", language)} - {winLossDate}]");

    private static void AddContent(StringBuilder messageBuilder, WinLossMaster winloss, string language)
    {
        messageBuilder
            .AppendLine($"<b>{TemplateHelper.Get(nameof(winloss.Turnover), language)}</b>: {winloss.Turnover.FormatDecimal()}")
            .AppendLine($"<b>{TemplateHelper.Get(nameof(winloss.GrossComm), language)}</b>: {winloss.GrossComm.FormatDecimal()}")
            .AppendLine($"<b>{TemplateHelper.Get(nameof(winloss.MasterTotal), language)}</b>: {winloss.MasterTotal.FormatDecimal()}")
            .AppendLine($"<b>{TemplateHelper.Get(nameof(winloss.SuperWinLoss), language)}</b>: {winloss.SuperWinLoss.FormatDecimal()}")
            .AppendLine($"<b>{TemplateHelper.Get(nameof(winloss.SuperComm), language)}</b>: {winloss.SuperComm.FormatDecimal()}")
            .AppendLine($"<b>{TemplateHelper.Get(nameof(winloss.SuperTotal), language)}</b>: {winloss.SuperTotal.FormatDecimal()}")
            .Append($"<b>{TemplateHelper.Get(nameof(winloss.CompanyWinLoss), language)}</b>: {winloss.CompanyWinLoss.FormatDecimal()}");
    }

    private static void AddContent(StringBuilder messageBuilder, WinLossAgent winloss, string language)
    {
        messageBuilder
            .AppendLine($"<b>{TemplateHelper.Get(nameof(winloss.Turnover), language)}</b>: {winloss.Turnover.FormatDecimal()}")
            .AppendLine($"<b>{TemplateHelper.Get(nameof(winloss.GrossComm), language)}</b>: {winloss.GrossComm.FormatDecimal()}")
            .AppendLine($"<b>{TemplateHelper.Get(nameof(winloss.AgentTotal), language)}</b>: {winloss.AgentTotal.FormatDecimal()}")
            .AppendLine($"<b>{TemplateHelper.Get(nameof(winloss.MasterWinLoss), language)}</b>: {winloss.MasterWinLoss.FormatDecimal()}")
            .AppendLine($"<b>{TemplateHelper.Get(nameof(winloss.MasterComm), language)}</b>: {winloss.MasterComm.FormatDecimal()}")
            .AppendLine($"<b>{TemplateHelper.Get(nameof(winloss.MasterTotal), language)}</b>: {winloss.MasterTotal.FormatDecimal()}")
            .Append($"<b>{TemplateHelper.Get(nameof(winloss.CompanyWinLoss), language)}</b>: {winloss.CompanyWinLoss.FormatDecimal()}");
    }

    private static void AddContent(StringBuilder messageBuilder, WinLossMember winloss, string language)
    {
        messageBuilder
            .AppendLine($"<b>{TemplateHelper.Get(nameof(winloss.Turnover), language)}</b>: {winloss.Turnover.FormatDecimal()}")
            .AppendLine($"<b>{TemplateHelper.Get(nameof(winloss.GrossComm), language)}</b>: {winloss.GrossComm.FormatDecimal()}")
            .AppendLine($"<b>{TemplateHelper.Get(nameof(winloss.MemberWinLoss), language)}</b>: {winloss.MemberWinLoss.FormatDecimal()}")
            .AppendLine($"<b>{TemplateHelper.Get(nameof(winloss.MemberComm), language)}</b>: {winloss.MemberComm.FormatDecimal()}")
            .AppendLine($"<b>{TemplateHelper.Get(nameof(winloss.MemberTotal), language)}</b>: {winloss.MemberTotal.FormatDecimal()}")
            .AppendLine($"<b>{TemplateHelper.Get(nameof(winloss.AgentWinLoss), language)}</b>: {winloss.AgentWinLoss.FormatDecimal()}")
            .AppendLine($"<b>{TemplateHelper.Get(nameof(winloss.AgentComm), language)}</b>: {winloss.AgentComm.FormatDecimal()}")
            .AppendLine($"<b>{TemplateHelper.Get(nameof(winloss.AgentTotal), language)}</b>: {winloss.AgentTotal.FormatDecimal()}")
            .Append($"<b>{TemplateHelper.Get(nameof(winloss.CompanyWinLoss), language)}</b>: {winloss.CompanyWinLoss.FormatDecimal()}");
    }

    public async Task<Result> Import(
        IEnumerable<ImportYesterdayWinLoss> notifications,
        DateTime winLossDate,
        string environment,
        int maxUserId)
    {
        var command = new ImportYesterdayWinLossCommand(
            BuildXMLData(notifications),
            winLossDate,
            environment,
            maxUserId);
        await dynamicRepository.ExecuteAsync(command);

        return new Result(command.ErrCode, command.ErrMsg);
    }

    private static string BuildXMLData(IEnumerable<ImportYesterdayWinLoss> notifications)
    {
        var xmlData = new StringBuilder();
        xmlData.Append("<root>");

        foreach (var item in notifications)
        {
            xmlData.Append(item.ToXml());
        }
            
        xmlData.Append("</root>");
        return xmlData.ToString();
    }
}