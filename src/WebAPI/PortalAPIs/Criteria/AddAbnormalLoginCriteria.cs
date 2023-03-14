using Fanex.Data;
using Fanex.Data.Repository;
using PortalAPIs.Models;

namespace PortalAPIs.Criteria;

public class AddAbnormalLoginCriteria : NonQueryCommand
{
    public AddAbnormalLoginCriteria(IEnumerable<AbnormalLoginModel> trans)
    {
        Trans = trans;
    }

    public string NewBatch
    {
        get
        {
            var result = "<ROOT>";

            foreach (var t in Trans)
            {
                result += $"<r TransID=\"{t.ID}\" TransTime=\"{t.TransTime}\" LoginName=\"{t.LoginName}\" UserAgent=\"{t.UserAgent}\" IP=\"{t.IP}\" Browser=\"{t.Browser}\" Device=\"{t.DeviceTypeName}\" OS=\"{t.OS}\" />";
            }

            result += "</ROOT>";

            return result;
        }
    }

    [SpParam(Ignored = true)]
    public IEnumerable<AbnormalLoginModel> Trans { get; }

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; }

    public string ExitsTransIDs { get; set; }

    public override string GetSettingKey()
        => "Acc_CustAbnormalLoginIns";

    public override bool IsValid() => Trans.Any();
}