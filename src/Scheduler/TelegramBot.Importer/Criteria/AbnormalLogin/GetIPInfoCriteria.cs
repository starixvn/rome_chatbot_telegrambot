using Fanex.Data.Repository;

namespace TelegramBot.Importer.Criteria.Announcement;

public class GetIPInfoCriteria : CriteriaBase
{
    public GetIPInfoCriteria(IEnumerable<string> ips)
    {
        IPs = string.Join(',', ips);
    }

    public string IPs { get; }

    public override string GetSettingKey() => "Acc_GetInfoListIPs";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(IPs);
}