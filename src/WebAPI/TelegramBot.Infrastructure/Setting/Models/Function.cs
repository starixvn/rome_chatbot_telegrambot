
using System;

namespace TelegramBot.Infrastructure.Setting.Models
{
    public class Function
    {
        public Function(short functionId, string functionName, byte type, bool status)
        {
            FunctionId = functionId;
            FunctionName = functionName;
            Type = type;
            Status = status;
        }
        public short FunctionId { get; }

        public string FunctionName { get; }

        public byte Type { get; }

        public bool Status { get; }
    }
}