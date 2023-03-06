using Fanex.Data.Repository;

namespace TelegramBot.Importer.Criteria.MemberWinLossLimits;

public class GetMemberWinLossLimitCommand : NonQueryCommand
{
    public GetMemberWinLossLimitCommand(int batchSize, long lastLogId, byte settingStatus)
    {
        BatchSize = batchSize;
        LastLogId = lastLogId;
        SettingStatus = settingStatus;
    }

    public long LastLogId { get; }

    public int BatchSize { get; }

    public long MaxLogId { get; set; }

    public byte SettingStatus { get; }

    public override string GetSettingKey() => "Age_Chatbot_CustWinLossReachLimitSel";

    public override bool IsValid() => BatchSize > 0;
}
