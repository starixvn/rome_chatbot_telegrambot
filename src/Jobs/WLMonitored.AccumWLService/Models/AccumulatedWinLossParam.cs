using System.Text;
using WLMonitored.Shared.Extensions;

namespace WLMonitored.AccumWLService.Models;

public record AccumulatedWinLossParam(
    int CustId,
    bool IsDeposit,
    decimal AccumWL,
    DateTime SettlementDate,
    string Action,
    byte RoleId
)
{
    private const char Credit = 'c';
    private const char Deposit = 'd';

    public void AppendCustXml(StringBuilder xmlBuilder)
        => xmlBuilder
        .Append("<Cust CustId=\"")
        .Append(CustId)
        .Append("\" AccumWL=\"")
        .Append(AccumWL)
        .Append("\" Type=\"")
        .Append(IsDeposit ? Deposit : Credit)
        .Append("\" SettlementDate=\"")
        .Append(SettlementDate.ToSimpleDate())
        .Append("\" Action=\"")
        .Append(Action)
        .Append("\" RoleId=\"")
        .Append(RoleId)
        .Append("\"/>");
}
