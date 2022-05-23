using System;
using TelegramBot.Infrastructure.Message.Constants;

namespace TelegramBot.Infrastructure.Message
{
    public interface IMessageFormatter
    {
        string NewLine();

        string Point();

        string Bold(string value);
    }

    public class TelegramFormatter : IMessageFormatter
    {
        public string NewLine() => "\n";

        public string Point() => "\u2022";

        public string Bold(string value) => $"<b>{value}</b>";
    }
}