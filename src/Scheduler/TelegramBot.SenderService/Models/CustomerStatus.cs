namespace TelegramBot.SenderService.Models;
/*
    [A]: Create
    [B]: Update
    [C]: Transfer
    [D]: Total Bets
    [E]: Reports
    [F]: Forcast
    [H]: Member Information
    [G]: View Log
    [L]: Bet Lists
*/
public record CustomerStatus(
    int CustId,
    int SubAccId,
    bool IsActive,
    bool IsProtectedMode,
    bool IsLocked,
    string PageAccess)
{
    public bool IsSubAcc => SubAccId != 0;

    public string GetStatus(string subAccPermission)
    {
        if (!IsActive)
        {
            return "Customer Status: Disabled or Closed";
        }

        if (IsProtectedMode)
        {
            return "Customer Status: Protected";
        }

        if (IsSubAcc)
        {
            if (!string.IsNullOrWhiteSpace(subAccPermission) && !PageAccess.Contains(subAccPermission))
            {
                return $"Customer Status: Sub-Account doesn't has {subAccPermission} permission";
            }
        }
        else if (IsLocked)
        {
            return "Customer Status: Locked";
        }

        return string.Empty;
    }
}