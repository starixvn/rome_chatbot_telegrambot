using Newtonsoft.Json;

namespace TelegramBot.Infrastructure.Command.Models
{
    public class CallbackData
    {
        public int Id { get; set; }


        [JsonProperty("t")]
        public string Text { get; set; }

        public bool IsStopUsingChatbot { get; set; }

        public bool IsMainMenu { get; set; }

        public string Cmd { get; set; }
    }
}