using System.ComponentModel;

namespace TelegramBot.Importer.Constants;

public enum FilterBy
{
    [Description("Created Date")]
    CreatedDate = 1,

    [Description("Release Date")]
    ReleasedDate = 2,

    [Description("Expiration Date")]
    ExpiredDate = 3
}