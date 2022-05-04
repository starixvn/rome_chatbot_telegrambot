namespace TelegramBot.Infrastructure.Command.Models
{
    public class CallBackData
    {
        public int ActionId { get; set; }

        public bool IsCancel { get; set; }

        public bool IsConfirmButton { get; set; }

        public bool IsBackToMainMenu { get; set; }

        public string CompiledCommand { get; set; }
    }
}