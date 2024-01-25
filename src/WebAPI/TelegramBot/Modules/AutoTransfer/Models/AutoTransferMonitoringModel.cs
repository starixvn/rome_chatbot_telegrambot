namespace TelegramBot.Modules.AutoTransfer.Models
{
    public class AutoTransferMonitoringModel
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal? TransferAmount { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
        public string CompletedTime { get; set; }
        public int CustId { get; set; }
        public bool IsAllowCancel { get; set; }

        public string TransferAmountStr => TransferAmount.HasValue ? TransferAmount.Value.ToString("#,##0.00") : "";
        public bool HasTransferAmount => TransferAmount.HasValue;

        public string FirstLastName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FirstName))
                {
                    return LastName;
                }

                return string.IsNullOrWhiteSpace(LastName) ? FirstName : $"{FirstName}/ {LastName}";
            }
        }
    }
}