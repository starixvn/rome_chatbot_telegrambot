namespace MemberAPIs.Requests;

public class AlertLoginFailedRequest
{
    public int LoginFailedType { get; set; }
    public int MemberId { get; set; }
    public string LastChangedPasswordDate { get; set; }
}