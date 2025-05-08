using MemberAPIs.Constants;

namespace MemberAPIs.Models;

public class APIResultModel
{
    public APIResultModel()
    {
        ErrCode = APIErrorCodeConstants.Success;
        ErrMsg = string.Empty;
    }

    public APIResultModel(string errCode, string errMsg)
    {
        ErrCode = errCode;
        ErrMsg = errMsg;
    }
    
    public string ErrCode { get; set; }
    public string ErrMsg { get; set; }

    public static APIResultModel Success()
    {
        return new APIResultModel();
    }

    public static APIResultModel Failed(string errMsg, string errCode = APIErrorCodeConstants.NotifyFailed)
    {
        return new APIResultModel(errCode, errMsg);
    }
}