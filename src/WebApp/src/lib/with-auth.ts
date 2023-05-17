import { NextApiHandler, NextApiRequest, NextApiResponse } from "next";
import { User } from "./session";

export default function withAuth(handler: NextApiHandler): NextApiHandler {
  return async (req: NextApiRequest, res: NextApiResponse) => {
    const { user, isSpecialSite } = req.session;
    if (user === undefined) {
      return res.status(401).end();
    }

    const umInfo = await getUmInfo();
    if (umInfo !== undefined && umInfo.isUnderMaintenanceTime && isSpecialSite) {
      return res.status(503).json({ umInfo: umInfo, isSpecialSite: isSpecialSite });
    }

    const customerStatus = await getCustomerStatus(user);
    if (!customerStatus.isActive || customerStatus.isProtectedMode || customerStatus.isLocked) {
      return res.status(403).json({ isActive: customerStatus.isActive, isLocked: customerStatus.isLocked, isProtectedMode: customerStatus.isProtectedMode });
    }

    try {
      return handler(req, res);
    } catch (error) {
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

  const url = `${process.env.UM_URL}/UnderMaintenance/Get`;
  const res = await fetch(url);
  return await res.json();
};

export const getCustomerStatus = async (user: User) => {
  const res = await fetch(`${process.env.CUSTOMER_API}/customer/${user.custId}/${user.subAccId}/status`);
  return await res.json();
};
