using Fanex.Data.Repository;

namespace TelegramBot.Importer.Criteria.MemberWinLossLimits;

public class GetMemberWinLossLimitCommand : NonQueryCommand
{
    public GetMemberWinLossLimitCommand(int batchSize, long lastLogId)
    {
        BatchSize = batchSize;
        LastLogId = lastLogId;
    }

    public long LastLogId { get; }

    public int BatchSize { get; }

    public long MaxLogId { get; set; }

    public override string GetSettingKey() => "Age_Chatbot_CustWinLossReachLimitSel";

    public override bool IsValid() => BatchSize > 0;
}
