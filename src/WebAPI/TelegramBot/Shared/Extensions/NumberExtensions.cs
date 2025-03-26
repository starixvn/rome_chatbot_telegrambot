namespace TelegramBot.Shared.Extensions
{
    public static class NumberExtensions
    {
        private const string NumberFormat = "#,##0.##";
        public const string NotAvaibleNumber = "N/A";

        public static string ToFormattedNumber(this decimal numberValue)
        {
            var result = numberValue.ToString(NumberFormat);

            return result;
        }

        public static string ToFormattedNumber(this decimal? numberValue)
            => numberValue.HasValue ? numberValue.Value.ToString(NumberFormat) : NotAvaibleNumber;
    }
}
