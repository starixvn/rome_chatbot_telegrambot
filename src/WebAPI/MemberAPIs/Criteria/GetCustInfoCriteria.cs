using Fanex.Data;
using Fanex.Data.Repository;

namespace MemberAPIs.Criteria;

public class GetCustInfoCriteria : NonQueryCommand
{
    public GetCustInfoCriteria(int custId)
    {
        CustId = custId;
    }

    public int CustId { get; }
    public string CustName { get; set; }
    public int UplineId { get; set; }
    public string UplineName { get; set; }
    public int FailedCount { get; set; }

    public override string GetSettingKey() => "API_LoginNotifyInfoSel";

    public override bool IsValid() => CustId > 0;
}