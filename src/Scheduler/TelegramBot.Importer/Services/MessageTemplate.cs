using System.Text;
using System.Xml.Linq;
using Alpha.API.Client.Models.WinLoss;
using TelegramBot.Importer.Constants;
using TelegramBot.Importer.Extensions;
using TelegramBot.Importer.Resources;

namespace TelegramBot.Importer.Services;

public interface IMessageTemplate
{
    string BuildPersonalMessage(string language, string currentDate, string content, string transId = "");

    string BuildYesterdayWinLossMessage(WinLossBase winloss, byte custLevel, string language, string winLossDate);
}

public class MessageTemplate : IMessageTemplate
{
    public string BuildPersonalMessage(string language, string currentDate, string content, string transId = "")
    {
        var messageBuilder = new StringBuilder();
        messageBuilder
            .AppendLine(BuildPersonalMessageTitle(language, currentDate))
            .Append(transId.Length == 0 ? content : content.Replace("@@", transId));

        return EncodeXml(messageBuilder.ToString());
    }

    private static string BuildPersonalMessageTitle(string language, string currentDate)
        => BuildTitle(TemplateHelper.Get("PersonalMessage", language), currentDate);

    public string BuildYesterdayWinLossMessage(WinLossBase winloss, byte custLevel, string language, string winLossDate)
    {
        var messageBuilder = new StringBuilder();
        messageBuilder
            .AppendLine(BuildYesterdayWinLossTitle(language, winLossDate));

        switch (custLevel)
        {
            case CustomerLevel.Super:
                AddMasterWinLossContent(messageBuilder, (WinLossMaster)winloss, language);
                break;

            case CustomerLevel.Master:
                AddAgentWinLossContent(messageBuilder, (WinLossAgent)winloss, language);
                break;

            case CustomerLevel.Agent:
                AddMemberWinLossContent(messageBuilder, (WinLossMember)winloss, language);
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(custLevel),
                    $"Not expected {nameof(custLevel)} value: {custLevel}");
        }

        return EncodeXml(messageBuilder.ToString());
    }

    private static string BuildYesterdayWinLossTitle(string language, string winLossDate)
        => BuildTitle(TemplateHelper.Get("YesterdayWinLoss", language), winLossDate);

    private static void AddMasterWinLossContent(StringBuilder messageBuilder, WinLossMaster winloss, string language)
        => messageBuilder
        .AppendLine(BuildContent(nameof(winloss.Turnover), language, winloss.Turnover))
        .AppendLine(BuildContent(nameof(winloss.GrossComm), language, winloss.GrossComm))
        .AppendLine(BuildContent(nameof(winloss.MasterTotal), language, winloss.MasterTotal))
        .AppendLine(BuildContent(nameof(winloss.SuperWinLoss), language, winloss.SuperWinLoss))
        .AppendLine(BuildContent(nameof(winloss.SuperComm), language, winloss.SuperComm))
        .AppendLine(BuildContent(nameof(winloss.SuperTotal), language, winloss.SuperTotal))
        .Append(BuildContent(nameof(winloss.CompanyWinLoss), language, winloss.CompanyWinLoss));

    private static void AddAgentWinLossContent(StringBuilder messageBuilder, WinLossAgent winloss, string language)
        => messageBuilder
        .AppendLine(BuildContent(nameof(winloss.Turnover), language, winloss.Turnover))
        .AppendLine(BuildContent(nameof(winloss.GrossComm), language, winloss.GrossComm))
        .AppendLine(BuildContent(nameof(winloss.AgentTotal), language, winloss.AgentTotal))
        .AppendLine(BuildContent(nameof(winloss.MasterWinLoss), language, winloss.MasterWinLoss))
        .AppendLine(BuildContent(nameof(winloss.MasterComm), language, winloss.MasterComm))
        .AppendLine(BuildContent(nameof(winloss.MasterTotal), language, winloss.MasterTotal))
        .Append(BuildContent(nameof(winloss.CompanyWinLoss), language, winloss.CompanyWinLoss));

    private static void AddMemberWinLossContent(StringBuilder messageBuilder, WinLossMember winloss, string language)
        => messageBuilder
        .AppendLine(BuildContent(nameof(winloss.Turnover), language, winloss.Turnover))
        .AppendLine(BuildContent(nameof(winloss.GrossComm), language, winloss.GrossComm))
        .AppendLine(BuildContent(nameof(winloss.MemberWinLoss), language, winloss.MemberWinLoss))
        .AppendLine(BuildContent(nameof(winloss.MemberComm), language, winloss.MemberComm))
        .AppendLine(BuildContent(nameof(winloss.MemberTotal), language, winloss.MemberTotal))
        .AppendLine(BuildContent(nameof(winloss.AgentWinLoss), language, winloss.AgentWinLoss))
        .AppendLine(BuildContent(nameof(winloss.AgentComm), language, winloss.AgentComm))
        .AppendLine(BuildContent(nameof(winloss.AgentTotal), language, winloss.AgentTotal))
        .Append(BuildContent(nameof(winloss.CompanyWinLoss), language, winloss.CompanyWinLoss));

    private static string BuildTitle(string title, string date)
        => $"[{title} - {date}]";

    private static string BuildContent(string resourceKey, string language, decimal value)
        => $"<b>{TemplateHelper.Get(resourceKey, language)}</b>: {value.FormatDecimal()}";

    public static string EncodeXml(string text, bool isAttribute = true, int attrSubStringLength = 5)
        => isAttribute
        ? new XAttribute("__n",text).ToString().Substring(attrSubStringLength).TrimEnd('\"')
        : new XElement("t", text).LastNode.ToString();
}
