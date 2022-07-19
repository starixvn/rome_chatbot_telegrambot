namespace TelegramBot.Importer.Models.Customers;
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

    public bool IsValid(string subAccPermission)
    {
        if (!IsActive || IsProtectedMode)
        {
            return false;
        }

        return IsSubAcc
            ? string.IsNullOrWhiteSpace(subAccPermission) || PageAccess.Contains(subAccPermission)
            : !IsLocked;
    }
}