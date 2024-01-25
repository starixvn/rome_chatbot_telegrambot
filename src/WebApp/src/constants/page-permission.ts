export enum SubAccountPermissions {
  Create = "[A]",
  Update = "[B]",
  Transfer = "[C]",
  TotalBets = "[D]",
  Reports = "[E]",
  Forcast = "[F]",
  MemberInformation = "[H]",
  ViewLog = "[G]",
  BetLists = "[L]",
}

export enum Pages {
  NewCustomer = "/newcustomer",
  ViewAutoTransferMonitoring = "/viewautotransfermonitor",
  EditAutoTransfer = "/editautotransfer",
}

export const PagePermission: { [key: string]: SubAccountPermissions } = {
  [Pages.NewCustomer]: SubAccountPermissions.Create,
  [Pages.ViewAutoTransferMonitoring]: SubAccountPermissions.Transfer,
  [Pages.EditAutoTransfer]: SubAccountPermissions.Transfer,
};
