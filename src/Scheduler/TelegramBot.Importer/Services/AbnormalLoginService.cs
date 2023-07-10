using Fanex.Data.Repository;
using TelegramBot.Importer.Constants;
using TelegramBot.Importer.Criteria.AbnormalLogin;
using TelegramBot.Importer.Criteria.Announcement;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Models.AbnormalLogin;

namespace TelegramBot.Importer.Services;

public interface IAbnormalLoginService
{
    Task<Tuple<long, IEnumerable<AbnormalLoginModel>>> GetAbnormalLogins(long maxLogId);
    Task<Result> Import(IEnumerable<AbnormalLoginNotification> notifications, string environment, long maxLogId);
    Task<Dictionary<string, IPInfoModel>> GetIPInfo(IEnumerable<string> ips);
}

public class AbnormalLoginService : NotificationServiceBase, IAbnormalLoginService
{
    public AbnormalLoginService(IDynamicRepository dynamicRepository) : base(dynamicRepository) { }

    public async Task<Tuple<long, IEnumerable<AbnormalLoginModel>>> GetAbnormalLogins(long maxLogId)
    {
        var command = new GetAbnormalLoginsCriteria(maxLogId);
        var abnormalLogins = await dynamicRepository.FetchAsync<AbnormalLoginModel>(command);

        return new Tuple<long, IEnumerable<AbnormalLoginModel>>(command.MaxLogId, abnormalLogins);
    }

    public async Task<Result> Import(
        IEnumerable<AbnormalLoginNotification> notifications,
        string environment,
        long maxLogId)
    {
        var command = new ImportAbnormalLoginsCommand(
            BuildXmlData(notifications),
            environment,
            FunctionKey.AbnormalLoginAlert);

        var result = await ExecuteNonQuery(command);

        return result;
    }

    public async Task<Dictionary<string, IPInfoModel>> GetIPInfo(IEnumerable<string> ips)
    {
        var criteria = new GetIPInfoCriteria(ips);

        var ipInfo = await dynamicRepository.FetchAsync<IPInfoModel>(criteria);
        var result = new Dictionary<string, IPInfoModel>();

        foreach (var info in ipInfo)
        {
            result.Add(info.IP, info);
        }

        return result;
    }
}