using Newtonsoft.Json;

namespace TelegramBot.Modules.Command.Models
{
    public class CallbackData
    {
        public int Id { get; set; }

        public bool IsStopUsingChatbot { get; set; }

        public bool IsMainMenu { get; set; }

        public string Cmd { get; set; }
    }
}