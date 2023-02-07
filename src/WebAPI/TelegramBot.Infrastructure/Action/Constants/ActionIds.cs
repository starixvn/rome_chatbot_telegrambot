namespace TelegramBot.Infrastructure.Action.Constants
{
    public static class ActionIds
    {
        public const short MainMenu = 0;
        public const short OutstandingEnterDownlineUsername = 9;
        public const short TopXOutstanding = 10;
        public const short TopXBalance = 17;
        public const short BalanceEnterDownlineUsername = 16;
        public const short Settings = 11;
        public const short TodayWinLossDownlineUsername = 21;
        public const short YesterdayWinLossDownlineUsername = 25;
        public const short Notifications = 26;
        public const short YesterdayWinLossNotification = 27;
        public const short PersonalMessageNotification = 28;
        public const short ProblemAccountNotification = 29;
        public const short MemberWinLossLimitNotification = 30;
        public const short ChangePasswordAlert = 31;
        public const short ChangeSecurityCodeAlert = 32;
        public const short WrongPasswordAlert = 33;
        public const short Language = 34;
        public const short English = 35;
        public const short ChineseTraditional = 36;
        public const short ChineseSimplified = 37;
        public const short Thai = 38;
        public const short Vietnamese = 39;
        public const short StopUsingChatBot = 40;
        public const short SystemAnnouncement = 41;
        public const short MyWatchlist = 42;
        public const short OTP = 43;
        public const short RiskAlert = 45;

        public static readonly short[] NotificationSettingActionIds = new[]{
            YesterdayWinLossNotification,
            PersonalMessageNotification,
            ProblemAccountNotification,
            MemberWinLossLimitNotification,
            ChangePasswordAlert,
            ChangeSecurityCodeAlert,
            WrongPasswordAlert,
            SystemAnnouncement,
            MyWatchlist,
            OTP,
            RiskAlert
        };

        public static readonly short[] LanguageSettingActionIds = new[]{
            English,
            ChineseTraditional,
            ChineseSimplified,
            Thai,
            Vietnamese
        };
    }
}
