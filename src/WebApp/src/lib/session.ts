import type { IronSessionOptions } from "iron-session";

export type User = {
  isLoggedIn: boolean;
  custId: number;
  subAccId: number;
  customerLevel: number;
  username: string;
  language: string;
  isVerifyOtpOrSC: boolean;
  hash: string;
  salt: string;
};

export type UMInfo = {
  isUnderMaintenanceTime: boolean;
  from: string;
  to: string;
};

export const sessionOptions: IronSessionOptions = {
  password: "2gyZ3GDw3LHZQKDhPmPDL3sjREVRXPr8",
  cookieName: "telegram-bot-webapp",
  cookieOptions: {
    maxAge: undefined,
    secure: process.env.NODE_ENV === "production",
  },
};

// This is where we specify the typings of req.session.*
declare module "iron-session" {
  interface IronSessionData {
    user?: User;
    umInfo?: UMInfo;
    isSpecialSite?: boolean;
    reChecking?: boolean;
  }
}
