using System.Collections.Generic;
using System.Linq;
using System.Text;
using TelegramBot.Common.Constants;
using TelegramBot.Common.Resources;

namespace TelegramBot.Infrastructure.Message
{
    public interface IBuilderService
    {
        string BuildUnderMaintenanceMessage(string from, string to);
    }

    public class BuilderService : IBuilderService
    {
        private static readonly List<string> unavailableLanguageCodes = new()
        {
            LanguageCodes.English,
            LanguageCodes.Chinese,
            LanguageCodes.Taiwan,
            LanguageCodes.Vietnamese
        };

        private static readonly List<string> underMaintenanceLanguageCodes = new()
        {
            LanguageCodes.English,
            LanguageCodes.Chinese,
            LanguageCodes.Taiwan
        };

        public string BuildUnderMaintenanceMessage(string from, string to)
        {
            return string.Join(
                "\n",
                underMaintenanceLanguageCodes.Select(langCode => 
                    string.Format(ResourceHelper.Get(nameof(SharedResource.UnderMaintenance), langCode), from, to)));
        }
    }
}
