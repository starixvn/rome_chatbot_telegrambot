using Fanex.Data.Repository;
using TelegramBot.Importer.Constants;

namespace TelegramBot.Importer.Criteria.Announcement;

public class GetAnnouncementCriteria : CriteriaBase
{
    public GetAnnouncementCriteria(DateTime fromDate, DateTime toDate, string environment)
    {
        FromDate = fromDate;
        ToDate = toDate;
        Environment = environment;
    }

    public string Environment { get; }

    public string System { get; } = string.Empty;

    public DateTime FromDate { get; }

    public DateTime ToDate { get; }

    public bool IsActive { get; } = true;

    public FilterBy FilterBy { get; } = FilterBy.ReleasedDate;


    public override string GetSettingKey() => "Age_Announcements_Select";

    public override bool IsValid() => FromDate <= ToDate;
}