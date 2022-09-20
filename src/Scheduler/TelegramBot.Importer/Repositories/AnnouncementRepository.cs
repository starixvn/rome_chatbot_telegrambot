using System.Data;
using System.Text;
using Alpha.Announcements.Models;
using Alpha.Announcements.Specs;
using Fanex.Data;
using Newtonsoft.Json;
using TelegramBot.Importer.Criteria.Announcement;
using TelegramBot.Importer.Extensions;
using TelegramBot.Importer.Helpers;
using TelegramBot.Importer.Models.Announcement;

namespace TelegramBot.Importer.Repositories;

public interface IAnnouncementRepository
{
    Task<IEnumerable<Announcement>> GetAnnouncements(GetAnnouncementCriteria criteria);
}

public class AnnouncementRepository : IAnnouncementRepository
{
    public async Task<IEnumerable<Announcement>> GetAnnouncements(GetAnnouncementCriteria criteria)
    {
        var announcements = new List<Announcement>();

        using var db = ObjectDbFactory.CreateInstance(criteria.GetSettingKey());
        using var reader = await db.ExecuteReaderAsync(criteria).ConfigureAwait(false);

        if (reader != null)
        {
            while (reader.Read())
            {
                announcements.Add(BuildAnnouncement(reader));
            }
        }

        return announcements;
    }

    private static Announcement BuildAnnouncement(IDataReader reader)
    {
        var encodedContents = Convert.FromBase64String(reader.ToString("Contents"));
        var plainText = Encoding.UTF8.GetString(encodedContents);

        return new Announcement
        {
            Code = reader.ToString("Code"),
            Contents = JsonConvert.DeserializeObject<IEnumerable<MessageContent>>(plainText),
            Environments = reader.ToString("Environments").Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                                    .Select(environment => environment.Trim()),
            Id = reader.ToInt("Id"),
            IsActive = reader.ToBool("IsActive"),
            Specifications = JsonConvert.DeserializeObject<IEnumerable<ISpecification<IParam>>>(
                        reader.ToString("Specifications"),
                        new SpecificationConverter()),
            Systems = reader.ToString("Systems").Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                                .Select(system => system.Trim()),
            Title = reader.ToString("Title"),
            ReleasedDate = reader.ToDateTime("FromDate"),
            ExpiredDate = reader.ToDateTime("ToDate"),
            CreatedDate = reader.ToDateTime("CreatedDate"),
        };
    }
}