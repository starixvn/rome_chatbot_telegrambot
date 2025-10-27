using Fanex.Data;
using Fanex.Data.Repository;

namespace TelegramBot.Importer.Criteria.AutoTransfer;

public class GetAutoTransferResultCriteria : CriteriaBase
{
    public GetAutoTransferResultCriteria(string siteDB, string environment)
    {
        SiteDB = siteDB;
        Environment = environment;
    }

    [SpParam(Ignored = true)]
    public string SiteDB { get; set; }

    public string Environment { get; set; }

    public override string GetSettingKey() => $"Age_AutoTransfer_Job_GetTelegramNotify_{SiteDB}";

    public override bool IsValid() => true;
}