using System.Text;

namespace WLMonitored.RiskControl.Importer.Models;

public record ReachPlayerHistoryParam(
    int CustId,
    decimal AccumWL,
    byte SportType,
    byte SettingType,
    decimal? WinningThreshold,
    decimal? LosingThreshold,
    byte ReachType
)
{
    public void AppendCustXml(StringBuilder xmlBuilder)
        => xmlBuilder
        .Append("<Cust CustId=\"")
        .Append(CustId)
        .Append("\" AccumWL=\"")
        .Append(AccumWL)
        .Append("\" SportType=\"")
        .Append(SportType)
        .Append("\" SettingType=\"")
        .Append(SettingType)
        .Append("\" WinningThreshold=\"")
        .Append(WinningThreshold)
        .Append("\" LosingThreshold=\"")
        .Append(LosingThreshold)
        .Append("\" ReachType=\"")
        .Append(ReachType)
        .Append("\"/>");
}