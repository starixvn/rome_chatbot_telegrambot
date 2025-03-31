namespace MemberAPIs.Params;

public class AlertLoginFailedParam
{
    public int LoginFailedType { get; set; }
    public int MemberId { get; set; }
    public string LastChangedPasswordDate { get; set; }
} 