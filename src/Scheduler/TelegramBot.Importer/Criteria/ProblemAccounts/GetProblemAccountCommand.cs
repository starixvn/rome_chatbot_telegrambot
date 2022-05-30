using Fanex.Data.Repository;

namespace TelegramBot.Importer.Criteria.ProblemAccounts;

public class GetProblemAccountCommand : NonQueryCommand
{
    public GetProblemAccountCommand(int lastLogIn, int batchSize)
    {
        LastLogIn = lastLogIn;
        BatchSize = batchSize;
    }

    public int LastLogIn { get; }

    public int BatchSize { get; }

    public int MaxLogId { get; set; }

    public override string GetSettingKey() => "Age_Chatbot_ProblemAccounts_Sel";

    public override bool IsValid() => BatchSize > 0;
}