using Fanex.Data.Repository;
using TelegramBot.Importer.Criteria.MemberWinLossLimits;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Models.MemberWinLossLimits;

namespace TelegramBot.Importer.Services;

public interface IMemberWinLossLimitService
{
    Task<Tuple<long, IEnumerable<MemberWinLossLimit>>> GetMemberWinLossLimits(int batchSize, long lastLogId);

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

    public Task<Result> Import(IEnumerable<MemberWinLossLimitNotification> notifications, string environment, long maxLogId)
        => ExecuteNonQuery(new ImportMemberWinLossLimitCommand(
            BuildXmlData(notifications),
            BuildXmlDataLog(notifications),
            environment,
            maxLogId
        ));
}
