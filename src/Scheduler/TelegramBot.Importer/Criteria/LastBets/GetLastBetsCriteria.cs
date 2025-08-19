using Fanex.Data.Repository;
using TelegramBot.Importer.Models.LastBets;

namespace TelegramBot.Importer.Criteria.LastBets;

public class GetLastBetsCriteria : NonQueryCommand
{
    public GetLastBetsCriteria(int custId, long lastTransId, FilterConfigurationModel filter)
    {
        CustId = custId;
        TransId = lastTransId;
        MemberList = filter.Members;
        BetTypeList = filter.FilterXml.BetType;
        SportTypeList = filter.Sports;
        LeagueList = filter.Leagues;
        LiveIndicator = filter.FilterXml.LiveStatus;
        MinStake = filter.FilterXml.Stake;
    }

    public int CustId { get; }

    public string MemberList { get; }

    public string BetTypeList { get; }

    public string SportTypeList { get; }

    public string LeagueList { get; }

    public byte LiveIndicator { get; }

    public decimal MinStake { get; }

    public long TransId { get; set; }

    public int BetCount { get; set; }

    public override string GetSettingKey()
        => "Age_BetList_LastBets_Ag_BetCount";

    public override bool IsValid()
        => !string.IsNullOrWhiteSpace(BetTypeList)
        && MinStake >= 0;
}