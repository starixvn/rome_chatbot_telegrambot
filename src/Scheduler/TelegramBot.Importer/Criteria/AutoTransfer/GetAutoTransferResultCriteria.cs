using Fanex.Data;
using Fanex.Data.Repository;

namespace TelegramBot.Importer.Criteria.AutoTransfer;

public class GetAutoTransferResultCriteria : CriteriaBase
{
    public GetAutoTransferResultCriteria(string siteDB)
    {
        SiteDB = siteDB;
    }

   [SpParam(Ignored = true)]
    public string SiteDB { get; set; }

    public override string GetSettingKey() => $"Age_AutoTransfer_Job_GetTelegramNotify_{SiteDB}";

    public override bool IsValid() => true;
}