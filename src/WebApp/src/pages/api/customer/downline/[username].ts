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
  const url = `${process.env.CUSTOMER_API}/customer/${user?.custId}/${user?.subAccId}/downlines/${username}`;
  const data = await fetchServerWrapper.get(url);
  res.json({ isDownLineUser: data.custId > 0 && isDirectDownLine(data.roleId, user?.customerLevel as number) });
}
