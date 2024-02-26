namespace TelegramBot.Importer.Models.LastBets;

public record SportModel
{
    public SportModel() { }

    public SportModel(int id, string name, IEnumerable<LeagueModel> leagues)
    {
        Id = id;
        Name = name;
        Leagues = leagues;
    }

    public int Id { get; }

    public string Name { get; }

    public IEnumerable<LeagueModel> Leagues { get; }
}