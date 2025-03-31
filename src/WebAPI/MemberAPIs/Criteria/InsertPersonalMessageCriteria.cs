using Fanex.Data;
using Fanex.Data.Repository;

namespace MemberAPIs.Criteria;

public class InsertPersonalMessageCriteria : NonQueryCommand
{
    private const int PersonalMessageType = 1;
    
    public InsertPersonalMessageCriteria(
        string contentEN,
        string contentCH,
        string contentCS,
        string contentTH,
        string contentVN,
        string contentJP,
        string contentKO)
    {
        ContentEN = contentEN;
        ContentCH = contentCH;
        ContentCS = contentCS;
        ContentTH = contentTH;
        ContentVN = contentVN;
        ContentJP = contentJP;
        ContentKO = contentKO;
    }

    [SpParam(SqlParamName = "content_en")]
    public string ContentEN { get; set; }

    [SpParam(SqlParamName = "content_ch")]
    public string ContentCH { get; set; }

    [SpParam(SqlParamName = "content_cs")]
    public string ContentCS { get; set; }

    [SpParam(SqlParamName = "content_jp")]
    public string ContentJP { get; set; }

    [SpParam(SqlParamName = "content_it")]
    public string ContentIT => string.Empty;

    [SpParam(SqlParamName = "content_th")]
    public string ContentTH { get; set; }

    [SpParam(SqlParamName = "content_ko")]
    public string ContentKO { get; set; }

    [SpParam(SqlParamName = "content_es")]
    public string ContentES => string.Empty;

    [SpParam(SqlParamName = "content_vn")]
    public string ContentVN { get; set; }

    [SpParam(SqlParamName = "msgtype")]
    public int MessageType { get; } = PersonalMessageType;

    public string CreatorName { get; } = string.Empty;

    public bool IsBatch { get; } = true;

    public int? MessageIdOutput { get; set; }

    public override string GetSettingKey() => "Age_Msg_SetMessage";

    public override bool IsValid() => true;
}
