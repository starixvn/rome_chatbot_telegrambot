using System;
using TelegramBot.Infrastructure.Message.Constants;

namespace TelegramBot.Infrastructure.Message
{
    public interface IMessageFormatter
    {
        string NewLine();

        string Point();
    }

    public class TelegramFormatter : IMessageFormatter
    {
        public string NewLine() => "\n";

        public string Point() => "\u2022";
    }
}