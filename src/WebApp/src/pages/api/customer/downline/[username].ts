import { fetchWrapper } from "./../../../../lib/fetch-wrapper";
import type { NextApiRequest, NextApiResponse } from "next";
import { withIronSessionApiRoute } from "iron-session/next";
import { sessionOptions } from "src/lib/session";

export default withIronSessionApiRoute(downLineRouter, sessionOptions);

async function downLineRouter(req: NextApiRequest, res: NextApiResponse) {
  const user = req.session.user;
  if (user) {
    const username = req.query["username"];
    const url = `${process.env.CUSTOMER_API}/customer/${user.custId}/${user.subAccId}/downlines/${username}`;
    const downLineUser = await fetchWrapper.get(url);
    res.status(200).json({ isDownLineUser: downLineUser.custId > 0 && user.custId !== downLineUser.custId });
  } else {
    res.status(500).json({});
  }
}
