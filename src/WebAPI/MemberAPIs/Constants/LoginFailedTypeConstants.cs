namespace MemberAPIs.Constants;

public static class LoginFailedTypeConstants
{
    public const int ChangedPassword = 1;
    public const int ReachFailedCount = 2;

    public static bool IsValidLoginFailedType(int value)
    {
        return value == ChangedPassword || value == ReachFailedCount;
    }
}