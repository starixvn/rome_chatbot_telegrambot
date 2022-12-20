namespace TelegramBot.Infrastructure.Shared.Extensions
{
    public static class NumberExtensions
    {
        private const string NumberFormat = "#,##0.##";

        public static string ToFormattedNumber(this decimal numberValue)
        {
            var result = numberValue.ToString(NumberFormat);

            return result;
        }
    }
}
