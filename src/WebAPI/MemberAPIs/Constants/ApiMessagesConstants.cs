namespace MemberAPIs.Constants
{
    public static class ApiMessagesConstants
    {
        public const string ApiKeyRequired = "API key is required";
        public const string ApiKeyInvalid = "API key is invalid";
        public const string InvalidLoginFailedType = "Invalid LoginFailedType";
        public const string InternalServerError = "An internal server error occurred while a request was processed.";
        public const string NotifyFailed = "Notify failed";
        public const string LoginFailedCountZero = "Login failed count is 0";
        public const string AccountIsLocked = "Account is locked";
        public const string LastChangedPasswordDateEmpty = "LastChangedPasswordDate is empty";
        public const string FailedCountNotThree = "FailedCount is not 3";
        public const string RateLimitExceeded = "Rate limit exceeded. Please try again later.";
        public const string LoginFailedTypeMismatchForReach3Attempts = "LoginFailedType should be 2 when FailedCount is 3.";
    }
}