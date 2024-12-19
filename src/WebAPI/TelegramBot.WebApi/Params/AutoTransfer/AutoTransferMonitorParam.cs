using System.ComponentModel.DataAnnotations;

namespace TelegramBot.WebApi.Params.AutoTransfer
{
    public class AutoTransferMonitorParam
    {
        [Required]
        public int CustId { get; set; }

        [Required]
        public string Language { get; set; }

        public string Username { get; set; }

        public string Status { get; set; }

        public int SortedBy { get; set; }

        public int Direction { get; set; }

        public byte LevelType { get; set; }
    }
}
