using Fanex.Data.Repository;

namespace TelegramBot.Importer.Criteria.ProblemAccounts;

public class GetProblemAccountCommand : NonQueryCommand
{
    public GetProblemAccountCommand(int lastLogId, int batchSize)
    {
        LastLogId = lastLogId;
        BatchSize = batchSize;
    }

    public int LastLogId { get; }

    public int BatchSize { get; }

    public int MaxLogId { get; set; }

    public override string GetSettingKey() => "Age_Chatbot_ProblemAccounts_Sel";

    public override bool IsValid() => BatchSize > 0;
}