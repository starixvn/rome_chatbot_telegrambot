using System.ComponentModel.DataAnnotations;

namespace TelegramBot.WebApi.Params
{
    public class CallbackQueryParam : CommandParam
    {
        [Required]
        public int MessageId { get; set; }

        [Required]
        public string CallbackId { get; set; }
    }
}