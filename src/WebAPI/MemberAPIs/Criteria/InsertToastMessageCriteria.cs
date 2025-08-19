using System.Globalization;
using Fanex.Data;
using Fanex.Data.Repository;
using MemberAPIs.Constants;
using MemberAPIs.Models;
using MemberAPIs.Resources;
using Newtonsoft.Json;

namespace MemberAPIs.Criteria;

public class InsertToastMessageCriteria : NonQueryCommand
{
    public InsertToastMessageCriteria(
        int targetId,
        int creator,
        string contentEN,
        string contentCH,
        string contentCS,
        string contentTH,
        string contentVN,
        string contentJP,
        string contentKO)
    {
        TypeId = ToastMessageTypeConstants.Warning;
        SiteFunctionId = ToastMessageFunctionConstants.FailedLogin;
        TargetId = targetId;
        Creator = creator;

        ContentEN = GetMessage(contentEN, LanguageCodeConstants.English);
        ContentCH = GetMessage(contentCH, LanguageCodeConstants.ChineseTraditional);
        ContentCS = GetMessage(contentCS, LanguageCodeConstants.ChineseSimplified);
        ContentTH = GetMessage(contentTH, LanguageCodeConstants.Thai);
        ContentVN = GetMessage(contentVN, LanguageCodeConstants.Vietnamese);
        ContentJP = GetMessage(contentJP, LanguageCodeConstants.Japanese);
        ContentKO = GetMessage(contentKO, LanguageCodeConstants.Korean);
    }

    public int TypeId { get; set; }
    public int SiteFunctionId { get; set; }
    public int TargetId { get; set; }
    public int Creator { get; set; }

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

    public int ErrCode { get; set; }
    public string ErrMsg { get; set; }
    public int ToastId { get; set; }

    public override string GetSettingKey() => "Age_ToastMessage_Ins";
    public override bool IsValid() => true;

    private static string GetMessage(string message, string language)
    {
        var culture = CultureInfo.CreateSpecificCulture(language);
        var content = new ToastContentModel
        {
            Title = Labels.ResourceManager.GetString("ToastTitle_DownlineLoginFailed", culture),
            Body = message
        };

        return JsonConvert.SerializeObject(content, Formatting.None);
    }
}
