import { NextApiHandler, NextApiRequest, NextApiResponse } from "next";
import { User } from "./session";

export default function withAuth(handler: NextApiHandler): NextApiHandler {
  return async (req: NextApiRequest, res: NextApiResponse) => {
    const { user, isSpecialSite } = req.session;
    if (user === undefined) {
      return res.status(401).end();
    }

    const isLinkBot = await checkIsLinkTelegramBot(user);
    if (!isLinkBot) {
      return res.status(401).end();
    }

    const umInfo = await getUmInfo();
    if (umInfo !== undefined && umInfo.isUnderMaintenanceTime && isSpecialSite) {
      return res.status(503).json({ umInfo: umInfo, isSpecialSite: isSpecialSite });
    }

    const customerStatus = await getCustomerStatus(user);
    if (!customerStatus.isActive || customerStatus.isProtectedMode || customerStatus.isLocked) {
      const key = !customerStatus.isActive ? "AccountClosed" : customerStatus.isLocked ? "AccountClocked" : "AccountProtected";
      return res.status(403).json({ key });
    }

    try {
      return handler(req, res);
    } catch (error) {
      return res.status(500).end();
    }
  };
}

export const getUmInfo = async () => {
  try {
    const isMaintain = process.env.IS_UNDER_MAINTENANCE_TIME;
    if (isMaintain === "true") {
      return {
        from: process.env.MAINTENANCE_TIME_FROM,
        to: process.env.MAINTENANCE_TIME_TO,
        isUnderMaintenanceTime: true,
        errorCode: 0,
      };
    }

    const url = `${process.env.UM_URL}/UnderMaintenance/Get`;
    const res = await fetch(url);
    return await res.json();
  } catch (error) {
    return {};
  }
};

export const getCustomerStatus = async (user: User): Promise<CustomerStatus> => {
  try {
    const res = await fetch(`${process.env.CUSTOMER_API}/customer/${user.custId}/${user.subAccId}/status`);
    return await res.json();
  } catch (error) {
    return {} as CustomerStatus;
  }
};

export const checkIsLinkTelegramBot = async (user: User): Promise<boolean> => {
  try {
    const body = { custId: user.custId, subAccId: user.subAccId, language: user.language };
    const requestOptions = {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: body && JSON.stringify(body),
    };
    const res = await fetch(`${process.env.TELEGRAM_API_URL}/Setting/custBotMessenger`, requestOptions);
    return await res.json();
  } catch (error) {
    return false;
  }
};

export enum Pages {
  NewCustomer = "/newcustomer",
}

export enum Permissions {
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

export const PagePermission = {
  "/newcustomer": Permissions.Create,
};

export type CustomerStatus = {
  custId: number;
  subAccId: number;
  isActive: boolean;
  isProtectedMode: boolean;
  isLocked: boolean;
  pageAccess: string;
  isSubAcc: boolean;
};

export const checkPagePermission = (customerPermission: string, permission: Permissions) => {
  if (!customerPermission) {
    return false;
  }
  return customerPermission.includes(permission);
};
