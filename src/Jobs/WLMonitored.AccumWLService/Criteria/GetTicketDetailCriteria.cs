using Fanex.Data.Repository;

namespace WLMonitored.AccumWLService.Criteria;

public sealed class GetTicketDetailCriteria : CriteriaBase
{
    public GetTicketDetailCriteria(long lastId, int batchSize)
    {
        LastId = lastId;
        BatchNo = batchSize;
    }

    public long LastId { get; }

    public int BatchNo { get; }


    public override string GetSettingKey()
        => "Acc_WLMonitored_Service_TicketDetailSel";

    public override bool IsValid()
        => LastId >= 0 && BatchNo > 0;
}