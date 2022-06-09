using Fanex.Data.Repository;

namespace WLMonitored.RiskControl.Importer.Criteria;

public sealed class ReachPlayerHistoryCommand : NonQueryCommand
{
    public ReachPlayerHistoryCommand(
        string custReached,
        byte notificationType,
        DateTime? lastCheckTime,
        DateTime? lastScanSettingDate,
        string serviceName)
    {
        CustReached = custReached;
        NotificationType = notificationType;
        LastCheckTime = lastCheckTime;
        LastScanSettingDate = lastScanSettingDate;
        ServiceName = serviceName;
    }

    public string CustReached { get; }

    public byte NotificationType { get; }

    public DateTime? LastCheckTime { get; }

    public DateTime? LastScanSettingDate { get; }

    public string ServiceName { get; }

    public override string GetSettingKey()
        => "Acc_WLMonitored_Service_ReachHistoryIns";

    public override bool IsValid()
        => !string.IsNullOrWhiteSpace(CustReached);
}