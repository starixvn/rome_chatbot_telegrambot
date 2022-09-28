using Fanex.Data.Repository;

namespace WLMonitored.AccumWLService.Criteria;

public sealed class UpdateAccumulateWinLossCommand : NonQueryCommand
{
    public UpdateAccumulateWinLossCommand(string custWinlost, long lastId, string serviceName)
    {
        CustWinlost = custWinlost;
        LastId = lastId;
        ServiceName = serviceName;
    }

    public string CustWinlost { get; }

    public long LastId { get; }

    public string ServiceName { get; }

    public override string GetSettingKey()
        => "Acc_WLMonitored_Service_AccumulatedData";

    public override bool IsValid()
        => !string.IsNullOrWhiteSpace(CustWinlost) && LastId > 0;
}