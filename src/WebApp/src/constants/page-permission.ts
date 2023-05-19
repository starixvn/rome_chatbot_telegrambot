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
}

export const PagePermission = {
  [Pages.NewCustomer]: SubAccountPermissions.Create,
};
