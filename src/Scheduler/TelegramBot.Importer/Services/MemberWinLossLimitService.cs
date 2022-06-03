using System.Text;
using Fanex.Data.Repository;
using TelegramBot.Importer.Constants;
using TelegramBot.Importer.Criteria.MemberWinLossLimits;
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
            .AppendLine(TemplateHelper.Get("MemberReachWinLossIntro", language));

        var isHaveMemberReachWinLimit = winLossLimitMessages.Any(m => m.Products.Any(p => p.CurrentWinLoss >= 0));
        if (isHaveMemberReachWinLimit)
        {
            messageBuilder.AppendLine($"{TemplateHelper.Get("MemberProductReachedWinLimit", language)}");
            messageBuilder.AppendLine($"{TemplateHelper.Get("MemberWinLimitNote", language)}");
            BuildProductReachedWinLimitContent(messageBuilder, winLossLimitMessages, custLevel);
        }

        var isHaveMemberReachLossLimit = winLossLimitMessages.Any(m => m.Products.Any(p => p.CurrentWinLoss < 0));
        if (isHaveMemberReachLossLimit)
        {
            messageBuilder.AppendLine($"{TemplateHelper.Get("MemberProductReachedLossLimit", language)}");
            messageBuilder.AppendLine($"{TemplateHelper.Get("MemberLossLimitNote", language)}");
            BuildProductReachedLossLimitContent(messageBuilder, winLossLimitMessages, custLevel);
        }

        return TemplateHelper.EncodeXml(messageBuilder.ToString());
    }

    private void BuildProductReachedWinLimitContent(
        StringBuilder messageBuilder,
        IEnumerable<WinLossLimitMessage> memberWinLossLimitMessages,
        byte custLevel)
    {
        foreach (var memberMessage in memberWinLossLimitMessages)
        {
            var products = memberMessage.Products.Where(p => p.CurrentWinLoss >= 0).ToList();
            if (products.Any())
            {
                BuildProductMesage(messageBuilder, memberMessage, products, custLevel);
            }
        }
    }

    private void BuildProductReachedLossLimitContent(
        StringBuilder messageBuilder,
        IEnumerable<WinLossLimitMessage> memberWinLossLimitMessages,
        byte custLevel)
    {
        foreach (var memberMessage in memberWinLossLimitMessages)
        {
            var products = memberMessage.Products.Where(p => p.CurrentWinLoss < 0).ToList();
            if (products.Any())
            {
                BuildProductMesage(messageBuilder, memberMessage, products, custLevel);
            }
        }
    }

    private void BuildProductMesage(
        StringBuilder messageBuilder,
        WinLossLimitMessage memberMessage,
        List<ProductWinLossLimitMessage> products,
        byte custLevel)
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

        var agentName = memberMessage.AUsername;
        messageBuilder.AppendLine($"<b>{usernames}</b>");
        foreach (var product in products)
        {
            messageBuilder.AppendLine($"{product.ProductName}. {product.CurrentWinLoss}. {product.MaximumWinLoss}. {product.CurrentWinLossPercentage}%");
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
