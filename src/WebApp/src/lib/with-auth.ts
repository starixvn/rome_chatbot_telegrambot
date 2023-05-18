import { NextApiHandler, NextApiRequest, NextApiResponse } from "next";
import { User } from "./session";

export default function withAuth(handler: NextApiHandler): NextApiHandler {
  return async (req: NextApiRequest, res: NextApiResponse) => {
    const { user, isSpecialSite } = req.session;
    if (user === undefined) {
      return res.status(401).end();
    }

    const isLinkBot = await checkIsLinkTelegramBot(user.tgChatId);
    if (isLinkBot === undefined) {
      return res.status(403).json({ key: "isUnLinkMessage" });
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

export const getCustomerStatus = async (user: User) => {
  try {
    const res = await fetch(`${process.env.CUSTOMER_API}/customer/${user.custId}/${user.subAccId}/status`);
    return await res.json();
  } catch (error) {
    return {};
  }
};

export const checkIsLinkTelegramBot = async (id: number) => {
  try {
    const res = await fetch(`${process.env.TELEGRAM_API_URL}/Setting/${id}`);
    return await res.json();
  } catch (error) {
    return {};
  }
};
