import { withIronSessionApiRoute } from "iron-session/next";
import type { NextApiRequest, NextApiResponse } from "next";
import { sessionOptions } from "src/lib/session";
import withAuth, { checkPagePermission } from "src/lib/with-auth";
import { SubAccountPermissions } from "src/constants/page-permission";
import { fetchServerWrapper } from "src/lib/fetch-server";

async function updateMultipleRouter(req: NextApiRequest, res: NextApiResponse) {
  const { user, pageAccess } = req.session;
  if (user?.subAccId !== 0) {
    const hasPagePermission = checkPagePermission(pageAccess as string, SubAccountPermissions.Transfer);
    if (!hasPagePermission) {
      return res.status(401).end();
    }
  }

  const url = `${process.env.TELEGRAM_API_URL}/AutoTransfer/UpdateMultiple`;
  const body = {
    ...req.body,
    uplineId: user?.custId,
    subAccId: user?.subAccId,
    uplineRoleId: user?.customerLevel,
    language: user?.language,
  };

  const data = await fetchServerWrapper.post(url, body);
  return res.json({ ...data });
};

export default withIronSessionApiRoute(withAuth(updateMultipleRouter), sessionOptions);