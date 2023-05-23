import type { NextApiRequest, NextApiResponse } from "next";
import { withIronSessionApiRoute } from "iron-session/next";
import { sessionOptions } from "src/lib/session";
import { fetchServerWrapper } from "src/lib/fetch-server";
import withAuth from "src/lib/with-auth";
import { isDirectDownLine } from "src/constants/role-id";

export default withIronSessionApiRoute(withAuth(downLineRouter), sessionOptions);

async function downLineRouter(req: NextApiRequest, res: NextApiResponse) {
  const user = req.session.user;
  const username = req.query["username"];

  const checkDownLineUrl = `${process.env.CUSTOMER_API}/customer/${user?.custId}/${user?.subAccId}/downlines/${username}`;
  const downLineInfo = await fetchServerWrapper.get(checkDownLineUrl);

  const checkStatusUrl = `${process.env.CUSTOMER_API}/customer/${downLineInfo.custId}/IsEnabled`;
  const isEnable = await fetchServerWrapper.get(checkStatusUrl);

  res.json({ isDownLineUser: downLineInfo.custId > 0 && isDirectDownLine(downLineInfo.roleId, user?.customerLevel as number), isEnable });
}
