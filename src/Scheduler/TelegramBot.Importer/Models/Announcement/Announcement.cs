namespace TelegramBot.Importer.Models.Announcement;

public class Announcement
{
    public string Code { get; set; } = default!;

    public IEnumerable<MessageContent> Contents { get; set; } = Enumerable.Empty<MessageContent>();

    public int Id { get; set; }

    public bool IsActive { get; set; }

    public IEnumerable<ISpecification<IParam>> Specifications { get; set; } = Enumerable.Empty<ISpecification<IParam>>();

    public string Title { get; set; } = default!;

    public IEnumerable<string> Environments { get; set; } = Enumerable.Empty<string>();

    public IEnumerable<string> Systems { get; set; } = Enumerable.Empty<string>();

    public DateTime ReleasedDate { get; set; }

    public DateTime ExpiredDate { get; set; }

    public DateTime CreatedDate { get; set; }
}