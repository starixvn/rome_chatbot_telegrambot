import type { NextApiRequest, NextApiResponse } from "next";
import { withIronSessionApiRoute } from "iron-session/next";
import { sessionOptions } from "src/lib/session";
import { fetchServerWrapper } from "src/lib/fetch-server";

export default withIronSessionApiRoute(downLineRouter, sessionOptions);

async function downLineRouter(req: NextApiRequest, res: NextApiResponse) {
  const user = req.session.user;
  const username = req.query["username"];
  const url = `${process.env.CUSTOMER_API}/customer/${user?.custId}/${user?.subAccId}/downlines/${username}`;
  const { resStatus, data } = await fetchServerWrapper.get(url);
  res.status(resStatus).json({ isDownLineUser: data.custId > 0 && user?.custId !== data.custId });
}
