import type { IronSessionOptions } from "iron-session";

export type User = {
  isLoggedIn: boolean;
  custId: number;
  subAccId: number;
  customerLevel: number;
  username: string;
  language: string;
  system: string;
  isVerifyOtpOrSC: boolean;
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
  }
}
