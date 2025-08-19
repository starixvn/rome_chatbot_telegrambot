namespace TelegramBot.WebApi.Params.AutoTransfer
{
    public class UpdateMultipleParam
    {
        public int UplineId { get; set; }
        public int SubAccId { get; set; }
        public int UplineRoleId { get; set; }
        public string Language { get; set; }

        public string OnIds { get; set; }
        public string OffIds { get; set; }
        public string OnNames { get; set; }
        public string OffNames { get; set; }
        public bool OnStatus { get; set; }
        public bool OffStatus { get; set; }
        public string OnSchedule { get; set; }
        public string OffSchedule { get; set; }
        public string OldTransferOption { get; set; }
    }
}
