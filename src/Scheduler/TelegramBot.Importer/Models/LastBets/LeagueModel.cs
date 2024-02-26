namespace TelegramBot.Importer.Models.LastBets;

public record LeagueModel
{
    public LeagueModel() { }

    public LeagueModel(int sportType, int leagueId, string leagueName = null)
    {
        SportType = sportType;
        LeagueId = leagueId;
        LeagueName = leagueName;
    }

    public int SportType { get; }

    public int LeagueId { get; }

    public string LeagueName { get; }
}