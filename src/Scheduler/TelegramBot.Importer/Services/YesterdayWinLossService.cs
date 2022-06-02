using System.Text;
using Alpha.API.Client.Models.WinLoss;
using Fanex.Data.Repository;
using TelegramBot.Importer.Constants;
using TelegramBot.Importer.Criteria.YesterdayWinLoss;
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
        .AppendLine(TemplateHelper.BuildContent(language, nameof(winloss.Turnover), winloss.Turnover))
        .AppendLine(TemplateHelper.BuildContent(language, nameof(winloss.GrossComm), winloss.GrossComm))
        .AppendLine(TemplateHelper.BuildContent(language, nameof(winloss.MasterTotal), winloss.MasterTotal))
        .AppendLine(TemplateHelper.BuildContent(language, nameof(winloss.SuperWinLoss), winloss.SuperWinLoss))
        .AppendLine(TemplateHelper.BuildContent(language, nameof(winloss.SuperComm), winloss.SuperComm))
        .AppendLine(TemplateHelper.BuildContent(language, nameof(winloss.SuperTotal), winloss.SuperTotal))
        .Append(TemplateHelper.BuildContent(language, nameof(winloss.CompanyWinLoss), winloss.CompanyWinLoss));

    private static void AddAgentWinLossContent(StringBuilder messageBuilder, string language, WinLossAgent winloss)
        => messageBuilder
        .AppendLine(TemplateHelper.BuildContent(language, nameof(winloss.Turnover), winloss.Turnover))
        .AppendLine(TemplateHelper.BuildContent(language, nameof(winloss.GrossComm), winloss.GrossComm))
        .AppendLine(TemplateHelper.BuildContent(language, nameof(winloss.AgentTotal), winloss.AgentTotal))
        .AppendLine(TemplateHelper.BuildContent(language, nameof(winloss.MasterWinLoss), winloss.MasterWinLoss))
        .AppendLine(TemplateHelper.BuildContent(language, nameof(winloss.MasterComm), winloss.MasterComm))
        .AppendLine(TemplateHelper.BuildContent(language, nameof(winloss.MasterTotal), winloss.MasterTotal))
        .Append(TemplateHelper.BuildContent(language, nameof(winloss.CompanyWinLoss), winloss.CompanyWinLoss));

    private static void AddMemberWinLossContent(StringBuilder messageBuilder, string language, WinLossMember winloss)
        => messageBuilder
        .AppendLine(TemplateHelper.BuildContent(language, nameof(winloss.Turnover), winloss.Turnover))
        .AppendLine(TemplateHelper.BuildContent(language, nameof(winloss.GrossComm), winloss.GrossComm))
        .AppendLine(TemplateHelper.BuildContent(language, nameof(winloss.MemberWinLoss), winloss.MemberWinLoss))
        .AppendLine(TemplateHelper.BuildContent(language, nameof(winloss.MemberComm), winloss.MemberComm))
        .AppendLine(TemplateHelper.BuildContent(language, nameof(winloss.MemberTotal), winloss.MemberTotal))
        .AppendLine(TemplateHelper.BuildContent(language, nameof(winloss.AgentWinLoss), winloss.AgentWinLoss))
        .AppendLine(TemplateHelper.BuildContent(language, nameof(winloss.AgentComm), winloss.AgentComm))
        .AppendLine(TemplateHelper.BuildContent(language, nameof(winloss.AgentTotal), winloss.AgentTotal))
        .Append(TemplateHelper.BuildContent(language, nameof(winloss.CompanyWinLoss), winloss.CompanyWinLoss));

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