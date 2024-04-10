using System.ComponentModel.DataAnnotations;

namespace TelegramBot.WebApi.Params.AutoTransfer
{
    public class AutoTransferCustomersParam
    {
        [Required]
        public int CustId { get; set; }
        
        [Required]
        public string Language { get; set; }

        public int CustLevel { get; set; }

        public string Username { get; set; }

        public string Status { get; set; }

        public string AutoTransferStatus { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }
    }
}
