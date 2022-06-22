using System.Text;
using Alpha.API.Client.Models.WinLoss;
using Fanex.Data.Repository;
using TelegramBot.Importer.Constants;
using TelegramBot.Importer.Criteria.YesterdayWinLoss;
using TelegramBot.Importer.Extensions;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Resources;
using ClientServices = Alpha.API.Client.Services;

namespace TelegramBot.Importer.Services;

public interface IYesterdayWinLossService
{
    Task<WinLossBase> GetWinLoss(int custId, byte custLevel, DateTime winLossDate);
    
    string BuildMessage(string language, WinLossBase winloss, byte custLevel, string winLossDate);

    Task<Result> Import(
        IEnumerable<Notification> notifications,
        DateTime winLossDate,
        string environment,
        int maxUserId);
}

public class YesterdayWinLossService : NotificationServiceBase, IYesterdayWinLossService
{
    private readonly ClientServices.IWinLossService winLossService;

    public YesterdayWinLossService(
        ClientServices.IWinLossService winLossService,
        IDynamicRepository dynamicRepository) : base(dynamicRepository)
        => this.winLossService = winLossService;

    public Task<WinLossBase> GetWinLoss(int custId, byte custLevel, DateTime winLossDate)
        => winLossService.GetWinLoss(custId, custLevel, winLossDate);

    public string BuildMessage(string language, WinLossBase winloss, byte custLevel, string winLossDate)
    {
        var messageBuilder = new StringBuilder();
        messageBuilder
            .AppendLine(TemplateHelper.BuildTitle(language, "YesterdayWinLoss", winLossDate));

        switch (custLevel)
        {
            case CustomerLevel.Super:
                AddMasterWinLossContent(messageBuilder, language, (WinLossMaster)winloss);
                break;

            case CustomerLevel.Master:
                AddAgentWinLossContent(messageBuilder, language, (WinLossAgent)winloss);
                break;

            case CustomerLevel.Agent:
                AddMemberWinLossContent(messageBuilder, language, (WinLossMember)winloss);
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(custLevel),
                    $"Not expected {nameof(custLevel)} value: {custLevel}");
        }

        return TemplateHelper.EncodeXml(messageBuilder.ToString());
    }

    private static void AddMasterWinLossContent(StringBuilder messageBuilder, string language, WinLossMaster winloss)
        => messageBuilder
        .AppendLine(TemplateHelper.BuildBoldContent(language, nameof(winloss.Turnover)))
        .AppendLine(FormatDecimalWithPadLeft(winloss.Turnover))
        .AppendLine(TemplateHelper.BuildBoldContent(language, nameof(winloss.GrossComm)))
        .AppendLine(FormatDecimalWithPadLeft(winloss.GrossComm))
        .AppendLine(TemplateHelper.BuildBoldContent(language, nameof(winloss.MasterTotal)))
        .AppendLine(FormatDecimalWithPadLeft(winloss.MasterTotal))
        .AppendLine(TemplateHelper.BuildBoldContent(language, nameof(winloss.SuperWinLoss)))
        .AppendLine(FormatDecimalWithPadLeft(winloss.SuperWinLoss))
        .AppendLine(TemplateHelper.BuildBoldContent(language, nameof(winloss.SuperComm)))
        .AppendLine(FormatDecimalWithPadLeft(winloss.SuperComm))
        .AppendLine(TemplateHelper.BuildBoldContent(language, nameof(winloss.SuperTotal)))
        .AppendLine(FormatDecimalWithPadLeft(winloss.SuperTotal))
        .AppendLine(TemplateHelper.BuildBoldContent(language, nameof(winloss.CompanyWinLoss)))
        .Append(FormatDecimalWithPadLeft(winloss.CompanyWinLoss));

    private static void AddAgentWinLossContent(StringBuilder messageBuilder, string language, WinLossAgent winloss)
        => messageBuilder
        .AppendLine(TemplateHelper.BuildBoldContent(language, nameof(winloss.Turnover)))
        .AppendLine(FormatDecimalWithPadLeft(winloss.Turnover))
        .AppendLine(TemplateHelper.BuildBoldContent(language, nameof(winloss.GrossComm)))
        .AppendLine(FormatDecimalWithPadLeft(winloss.GrossComm))
        .AppendLine(TemplateHelper.BuildBoldContent(language, nameof(winloss.AgentTotal)))
        .AppendLine(FormatDecimalWithPadLeft(winloss.AgentTotal))
        .AppendLine(TemplateHelper.BuildBoldContent(language, nameof(winloss.MasterWinLoss)))
        .AppendLine(FormatDecimalWithPadLeft(winloss.MasterWinLoss))
        .AppendLine(TemplateHelper.BuildBoldContent(language, nameof(winloss.MasterComm)))
        .AppendLine(FormatDecimalWithPadLeft(winloss.MasterComm))
        .AppendLine(TemplateHelper.BuildBoldContent(language, nameof(winloss.MasterTotal)))
        .AppendLine(FormatDecimalWithPadLeft(winloss.MasterTotal))
        .AppendLine(TemplateHelper.BuildBoldContent(language, nameof(winloss.CompanyWinLoss)))
        .Append(FormatDecimalWithPadLeft(winloss.CompanyWinLoss));

    private static void AddMemberWinLossContent(StringBuilder messageBuilder, string language, WinLossMember winloss)
        => messageBuilder
        .AppendLine(TemplateHelper.BuildBoldContent(language, nameof(winloss.Turnover)))
        .AppendLine(FormatDecimalWithPadLeft(winloss.Turnover))
        .AppendLine(TemplateHelper.BuildBoldContent(language, nameof(winloss.GrossComm)))
        .AppendLine(FormatDecimalWithPadLeft(winloss.GrossComm))
        .AppendLine(TemplateHelper.BuildBoldContent(language, nameof(winloss.MemberWinLoss)))
        .AppendLine(FormatDecimalWithPadLeft(winloss.MemberWinLoss))
        .AppendLine(TemplateHelper.BuildBoldContent(language, nameof(winloss.MemberComm)))
        .AppendLine(FormatDecimalWithPadLeft(winloss.MemberComm))
        .AppendLine(TemplateHelper.BuildBoldContent(language, nameof(winloss.MemberTotal)))
        .AppendLine(FormatDecimalWithPadLeft(winloss.MemberTotal))
        .AppendLine(TemplateHelper.BuildBoldContent(language, nameof(winloss.AgentWinLoss)))
        .AppendLine(FormatDecimalWithPadLeft(winloss.AgentWinLoss))
        .AppendLine(TemplateHelper.BuildBoldContent(language, nameof(winloss.AgentComm)))
        .AppendLine(FormatDecimalWithPadLeft(winloss.AgentComm))
        .AppendLine(TemplateHelper.BuildBoldContent(language, nameof(winloss.AgentTotal)))
        .AppendLine(FormatDecimalWithPadLeft(winloss.AgentTotal))
        .Append(TemplateHelper.BuildBoldContent(language, nameof(winloss.CompanyWinLoss)))
        .AppendLine(FormatDecimalWithPadLeft(winloss.CompanyWinLoss));

    private static string FormatDecimalWithPadLeft(decimal value)
        => $"    {value.FormatDecimal()}";

    public Task<Result> Import(
        IEnumerable<Notification> notifications,
        DateTime winLossDate,
        string environment,
        int maxUserId)
        => ExecuteNonQuery(new ImportYesterdayWinLossCommand(
            BuildXmlData(notifications),
            winLossDate,
            environment,
            maxUserId));
}