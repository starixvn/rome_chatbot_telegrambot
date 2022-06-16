using System.Text;
using Fanex.Data.Repository;
using TelegramBot.Importer.Constants;
using TelegramBot.Importer.Criteria.MemberWinLossLimits;
using TelegramBot.Importer.Extensions;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Models.MemberWinLossLimits;
using TelegramBot.Importer.Resources;

namespace TelegramBot.Importer.Services;

public interface IMemberWinLossLimitService
{
    Task<Tuple<long, IEnumerable<MemberWinLossLimit>>> GetMemberWinLossLimits(int batchSize, long lastLogId);

    string BuildMessage(
        IEnumerable<WinLossLimitMessage> winLossLimitMessages,
        string language,
        byte custLevel,
        string currentDate);

    Task<Result> Import(
        IEnumerable<MemberWinLossLimitNotification> notifications,
        string environment,
        long maxLogId);
}

public class MemberWinLossLimitService : NotificationServiceBase, IMemberWinLossLimitService
{
    public MemberWinLossLimitService(IDynamicRepository dynamicRepository)
        : base(dynamicRepository)
    {
    }

    public async Task<Tuple<long, IEnumerable<MemberWinLossLimit>>> GetMemberWinLossLimits(int batchSize, long lastLogId)
    {
        var command = new GetMemberWinLossLimitCommand(batchSize, lastLogId);
        var memberWinLossLimits = await dynamicRepository.FetchAsync<MemberWinLossLimit>(command);

        return new Tuple<long, IEnumerable<MemberWinLossLimit>>(command.MaxLogId, memberWinLossLimits);
    }

    public string BuildMessage(IEnumerable<WinLossLimitMessage> winLossLimitMessages, string language, byte custLevel, string currentDate)
    {
        var messageBuilder = new StringBuilder();
        messageBuilder
            .AppendLine(TemplateHelper.BuildTitle(language, "MemberWinLossLimitAlert", currentDate))
            .AppendLine(TemplateHelper.Get("MemberReachWinLossIntro", language))
            .AppendLine($"<b>{TemplateHelper.Get("MemberWinLossLimitNote", language)}</b>");

        var sortedWinLossLimitMessage = winLossLimitMessages.OrderBy(w => w.Username);
        foreach (var memberMessage in sortedWinLossLimitMessage)
        {
            BuildUsernames(messageBuilder, memberMessage, custLevel);
            BuildWinProducts(messageBuilder, memberMessage.Products.Where(p => p.CurrentWinLoss > 0));
            BuildLossProducts(messageBuilder, memberMessage.Products.Where(p => p.CurrentWinLoss < 0));
        }

        return TemplateHelper.EncodeXml(messageBuilder.ToString());
    }

    private static void BuildUsernames(StringBuilder messageBuilder, WinLossLimitMessage memberMessage, byte custLevel)
    {
        var usernames = custLevel switch
        {
            CustomerLevel.Agent => memberMessage.Username,
            CustomerLevel.Master => $"{memberMessage.AUsername} | {memberMessage.Username}",
            CustomerLevel.Super => $"{memberMessage.MUsername} | {memberMessage.AUsername} | {memberMessage.Username}",
            _ => throw new ArgumentOutOfRangeException(
                nameof(custLevel),
                $"Not expected {nameof(custLevel)} value: {custLevel}")
        };

        messageBuilder.AppendLine($"<b>{usernames}</b>");
    }

    private static void BuildWinProducts(StringBuilder messageBuilder, IEnumerable<ProductWinLossLimitMessage> products)
    {
        foreach (var product in products)
        {
            messageBuilder.AppendLine($"{product.ProductName}. {product.CurrentWinLoss.FormatDecimal()} | {product.MaximumWinLoss.FormatDecimalToInt()}X | {product.CurrentWinLossPercentage.FormatDecimalUptoTwoDigit()}%");
        }
    }

    private static void BuildLossProducts(StringBuilder messageBuilder, IEnumerable<ProductWinLossLimitMessage> products)
    {
        foreach (var product in products)
        {
            messageBuilder.AppendLine($"{product.ProductName}. {product.CurrentWinLoss.FormatDecimal()} | -{product.MaximumWinLoss.FormatDecimal()} | {product.CurrentWinLossPercentage.FormatDecimalUptoTwoDigit()}%");
        }
    }

    public Task<Result> Import(IEnumerable<MemberWinLossLimitNotification> notifications, string environment, long maxLogId)
        => ExecuteNonQuery(new ImportMemberWinLossLimitCommand(
            BuildXmlData(notifications),
            BuildXmlDataLog(notifications),
            environment,
            maxLogId
        ));
}
