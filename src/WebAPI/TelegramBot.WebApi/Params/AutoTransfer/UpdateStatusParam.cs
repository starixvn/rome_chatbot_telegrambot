namespace TelegramBot.WebApi.Params.AutoTransfer
{
    public class UpdateStatusParam
    {
        public int UplineId { get; set; }
        public int SubAccId { get; set; }
        public int UplineRoleId { get; set; }        
        public string Language { get; set; }

        public int CustId { get; set; }
        public string Username { get; set; }
        public bool State { get; set; }
        public string Schedule { get; set; }
        public string OldTransferOption { get; set; }

        public string[] ScheduleDates => (Schedule ?? string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        public bool IsValid() => UplineId > 0 && CustId > 0 &&  (!State || ScheduleDates.Any());
    }
}
