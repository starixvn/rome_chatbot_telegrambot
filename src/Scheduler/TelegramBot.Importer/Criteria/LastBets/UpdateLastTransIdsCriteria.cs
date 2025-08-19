using Fanex.Data.Repository;
using Newtonsoft.Json;

namespace TelegramBot.Importer.Criteria.LastBets;

public class UpdateLastTransIdsCriteria : NonQueryCommand
{
    public UpdateLastTransIdsCriteria(Dictionary<string, long> lastTransIds, string environment)
    {
        Environment = environment;
        ListTransID = JsonConvert.SerializeObject(lastTransIds);
    }

    public string Environment { get; }

    public string ListTransID { get; set; }

    public override string GetSettingKey()
        => "Age_BetList_LastBets_LastTransIdsUpd";

    public override bool IsValid() => true;
}