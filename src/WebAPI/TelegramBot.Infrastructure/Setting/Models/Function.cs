
namespace TelegramBot.Infrastructure.Setting.Models
{
    public class Function
    {
        public Function(short functionId, string functionName, byte type, bool status)
        {

        }
        public short FunctionId { get; }

        public string FunctionName { get; }

        public byte Type { get; }

        public bool Status { get; }
    }
}