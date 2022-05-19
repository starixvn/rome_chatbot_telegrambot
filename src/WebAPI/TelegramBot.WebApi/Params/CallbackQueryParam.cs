using System.ComponentModel.DataAnnotations;
using TelegramBot.Infrastructure.Command.Models;

namespace TelegramBot.WebApi.Params
{
    public class CallbackQueryParam
    {
        [Required]
        public long MessengerBotId { get; set; }

        [Required]
        public long ChatId { get; set; }

        [Required]
        public int MessageId { get; set; }

        [Required]
        public string CallbackId { get; set; }

        [Required]
        public string CallbackData { get; set; }
    }
}