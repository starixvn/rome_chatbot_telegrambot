using Fanex.Data.Repository;

namespace TelegramBot.Importer.Criteria.LastBets;

public class GetLastTransIdsCriteria : CriteriaBase
{
    public GetLastTransIdsCriteria(string environment)
    {  
        Environment = environment;
    }

    public string Environment { get; }

    public override string GetSettingKey()
        => "Age_BetList_LastBets_LastTransIdsSel";

    public override bool IsValid() => true;
}