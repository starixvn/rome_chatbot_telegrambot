import type { NextApiRequest, NextApiResponse } from "next";
import { withIronSessionApiRoute } from "iron-session/next";
import { sessionOptions } from "src/lib/session";
import { fetchWrapper } from "src/lib/fetch-wrapper";

export default withIronSessionApiRoute(generateRouter, sessionOptions);

async function generateRouter(req: NextApiRequest, res: NextApiResponse) {
  if (req.method !== "POST") {
    res.status(405).send("Method not allowed");
    return;
  }

  const { user } = req.session;

  if (!user || user.isLoggedIn === false) {
    res.status(401).end();
    return;
  }

  const url = `${process.env.CUSTOMER_API}/customer/otp/telegram/generate`;
  await fetchWrapper.post(url, { custId: user.custId, subAccId: user.subAccId });
  res.status(200).json({});
}
