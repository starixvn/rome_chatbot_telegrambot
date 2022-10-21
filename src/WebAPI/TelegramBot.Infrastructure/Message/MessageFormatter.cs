namespace TelegramBot.Infrastructure.Message
{
    public interface IMessageFormatter
    {
        string NewLine();

        string Point();

        string Bold(string value);

        string Center(string value, int width);
    }

    public class TelegramFormatter : IMessageFormatter
    {
        public string NewLine() => "\n";

        public string Point() => "\u2022";

        public string Bold(string value) => $"<b>{value}</b>";

        public string Center(string value, int width)
        {
            var padding = (width + value.Length) / 2;
            return value.PadLeft(padding, ' ');
        }
    }
}