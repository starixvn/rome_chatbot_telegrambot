import { NextApiHandler, NextApiRequest, NextApiResponse } from "next";
import * as Sentry from "@sentry/nextjs";
import { User } from "./session";
import { SubAccountPermissions } from "src/constants/page-permission";

export type CustomerStatus = {
  custId: number;
  subAccId: number;
  isActive: boolean;
  isProtectedMode: boolean;
  isLocked: boolean;
  pageAccess: string;
  isSubAcc: boolean;
};

export default function withAuth(handler: NextApiHandler): NextApiHandler {
  return async (req: NextApiRequest, res: NextApiResponse) => {
    const { user, isSpecialSite } = req.session;
    try {
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
      return handler(req, res);
    } catch (error) {
      Sentry.captureException(error);
      return res.status(500).end();
    }
  };
}

export const getUmInfo = async () => {
  const isMaintain = process.env.IS_UNDER_MAINTENANCE_TIME;
  if (isMaintain === "true") {
    return {
      from: process.env.MAINTENANCE_TIME_FROM,
      to: process.env.MAINTENANCE_TIME_TO,
      isUnderMaintenanceTime: true,
      errorCode: 0,
    };
  }

  const url = `${process.env.UM_URL}/UnderMaintenance/Get?siteId=${process.env.UM_SITE_ID || 3400000}`;
  const res = await fetch(url);
  return await res.json();
};

export const getCustomerStatus = async (user: User): Promise<CustomerStatus> => {
  const res = await fetch(`${process.env.CUSTOMER_API}/customer/${user.custId}/${user.subAccId}/status`);
  return await res.json();
};

export const checkIsLinkTelegramBot = async (user: User): Promise<boolean> => {
  // try catch for success post request but not return data
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

export const checkPagePermission = (customerPermission: string, permission: SubAccountPermissions) => {
  if (!customerPermission || !permission) {
    return false;
  }
  return customerPermission.includes(permission);
};
