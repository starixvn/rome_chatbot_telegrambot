namespace TelegramBot.Modules.AutoTransfer.Models
{
    public class AutoTransferMonitoringPageModel
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public IEnumerable<AutoTransferMonitoringModel> Transfers { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public int SortedBy { get; set; }
        public int Direction { get; set; }

        public int TotalCancelAll { get; set; }
        public int TotalPending { get; set; }
        public int TotalProcessing { get; set; }
        public int TotalFailed { get; set; }
    }
}