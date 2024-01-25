
namespace TelegramBot.Modules.AutoTransfer.Params
{
    public class UpdateSingleParam 
    {
         public int CustId { get; set; }

        public int UplineId { get; set; }

        public string IP { get; set; }

        public string Language { get; set; }

        public bool Status { get; set; }

        public string Schedule { get; set; }

        public int SubAccId { get; set; }

        public string OldTransferOption { get; set; }

        public string Username { get; set; }

        public int UplineRoleId { get; set; }
    }
}