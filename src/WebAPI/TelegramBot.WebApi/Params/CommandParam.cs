using System.ComponentModel.DataAnnotations;
using TelegramBot.Infrastructure.Command.Models;

namespace TelegramBot.WebApi.Params
{
    public class CommandParam
    {
        [Required]
        public long MessengerBotId { get; set; }

        [Required]
        public long ChatId { get; set; }

        [Required]
        public string Message { get; set; }
    }
}
