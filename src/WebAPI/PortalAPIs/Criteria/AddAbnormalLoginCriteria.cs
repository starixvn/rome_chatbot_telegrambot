using Fanex.Data.Repository;

namespace PortalAPIs.Criteria;

public class AddAbnormalLoginCriteria : NonQueryCommand
{
    public AddAbnormalLoginCriteria(string batch)
    {
        NewBatch = batch;
    }

    public string NewBatch { get; }

    public string ExitsTransIDs { get; set; }
    public int ErrCode { get; set; }
    public string ErrMsg { get; set; }

    public override string GetSettingKey() => "Acc_CustAbnormalLoginIns";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(NewBatch);
}