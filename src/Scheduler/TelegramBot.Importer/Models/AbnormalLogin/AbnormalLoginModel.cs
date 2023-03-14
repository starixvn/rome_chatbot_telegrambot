namespace TelegramBot.Importer.Models.AbnormalLogin;

public record AbnormalLoginModel
{
    public long LoginID { get; set; }
    public int CustId { get; set; }
    public int SubAccId { get; set; }
    public string Device { get; set; }
    public string Browser { get; set; }
    public string IP { get; set; }
    public string LoginTime { get; set; }
    public int Status { get; set; }
    public int Count { get; set; }
    public string LoginName { get; set; }
    public string OS { get; set; }
    public string UserAgent { get; set; }

    public string Language { get; set; }
    public int UserId { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
}
