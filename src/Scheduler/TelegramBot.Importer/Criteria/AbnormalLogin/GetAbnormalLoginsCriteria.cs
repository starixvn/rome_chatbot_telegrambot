using Fanex.Data.Repository;

namespace TelegramBot.Importer.Criteria.Announcement;

public class GetAbnormalLoginsCriteria : CriteriaBase
{
    public GetAbnormalLoginsCriteria(long oldMaxLogId)
    {
        OldMaxLogID = oldMaxLogId;
    }

    public long OldMaxLogID { get; }

    public long MaxLogId { get; set; }

    public override string GetSettingKey() => "Acc_CustAbnormalLoginSel";

    public override bool IsValid() => true;
}