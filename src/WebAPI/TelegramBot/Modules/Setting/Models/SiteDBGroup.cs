using TelegramBot.Modules.Function.Models;

namespace TelegramBot.Modules.Setting.Models
{
    public class SiteDBGroup
    {
        public int SiteDBGroupID { get; set; }

        public int CustDBGroupID { get; set; }

        public string LinkedServerName { get; set; }

        public string ClientAccessName { get; set; }

        public string AGIP { get; set; }
    }
}