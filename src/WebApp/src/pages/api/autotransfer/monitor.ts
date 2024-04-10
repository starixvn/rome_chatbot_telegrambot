import { withIronSessionApiRoute } from "iron-session/next";
import type { NextApiRequest, NextApiResponse } from "next";
import { sessionOptions, User } from "src/lib/session";
import withAuth, { checkPagePermission } from "src/lib/with-auth";
import { SubAccountPermissions } from "src/constants/page-permission";
import { fetchServerWrapper } from "src/lib/fetch-server";

async function monitorRouter(req: NextApiRequest, res: NextApiResponse) {
  const { user, pageAccess } = req.session;
  if (user?.subAccId !== 0) {
    const hasPagePermission = checkPagePermission(pageAccess as string, SubAccountPermissions.Transfer);
    if (!hasPagePermission) {
      return res.status(401).end();
    }
  }
 
  const url = `${process.env.TELEGRAM_API_URL}/AutoTransfer/Monitor`;
  const body = {
    ...req.body,
    custId: user?.custId,
    language: user?.language,
  };

  const monitorData = await fetchServerWrapper.post(url, body);
  return res.json({ ...monitorData });
}

export default withIronSessionApiRoute(withAuth(monitorRouter), sessionOptions);