namespace TelegramBot.Importer.Models.MemberWinLossLimits;

public record MemberWinLossLimit
{
    public MemberWinLossLimit()
    {
    }

#pragma warning disable S107
    public MemberWinLossLimit(
        int custId,
        string username,
        long logId,
        decimal winLost,
        decimal winLostLimit,
        decimal winLossPercent,
        int sCustId,
        int mCustId,
        int aCustId,
        string sUsername,
        string mUsername,
        string aUsername,
        int productId)
#pragma warning restore S107
    {
        CustId = custId;
        Username = username;
        LogId = logId;
        WinLost = winLost;
        WinLossLimit = winLostLimit;
        WinLossPercent = winLossPercent;
        SCustId = sCustId;
        MCustId = mCustId;
        ACustId = aCustId;
        SUsername = sUsername;
        MUsername = mUsername;
        AUsername = aUsername;
        ProductId = productId;
    }

    public int CustId { get; }

    public string Username { get; } = default!;

    public long LogId { get; }

    public decimal WinLost { get; }

    public decimal WinLossLimit { get; }

    public decimal WinLossPercent { get; }

    public int SCustId { get; }

    public int MCustId { get; }

    public int ACustId { get; }

    public string SUsername { get; } = default!;

    public string MUsername { get; } = default!;

    public string AUsername { get; } = default!;

    public int ProductId { get; }
}