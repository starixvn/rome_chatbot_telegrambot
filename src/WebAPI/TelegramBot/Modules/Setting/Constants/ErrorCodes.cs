namespace TelegramBot.Modules.Setting.Constants
{
    public static class ErrorCodes
    {
        public const int OTP_Expired = -1;
        public const int OTP_Invalid = -2;
        public const int OTP_FailedManyTimes = -3;
        public const int OTP_NotValidated = -4;

        public const int MessengerNotFound = 102;

        public const int AccountAlreadyLinked = 103;
        public const int TelegramAccountAlreadyLinked = 104;
        public const int BadRequest = 400;
        public const int ADBTChanged = 108;
    }
}