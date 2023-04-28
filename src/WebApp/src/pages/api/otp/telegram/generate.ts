import type { NextApiRequest, NextApiResponse } from "next";
import { withIronSessionApiRoute } from "iron-session/next";
import { sessionOptions } from "src/lib/session";
import { fetchServerWrapper } from "src/lib/fetch-server";
import withAuth from "src/lib/with-auth";

export default withIronSessionApiRoute(withAuth(generateRouter), sessionOptions);

async function generateRouter(req: NextApiRequest, res: NextApiResponse) {
  if (req.method !== "POST") {
    res.status(405).send("Method not allowed");
    return;
  }
  const { user } = req.session;

  const url = `${process.env.CUSTOMER_API}/customer/otp/telegram/generate`;
  const data = await fetchServerWrapper.post(url, { custId: user?.custId, subAccId: user?.subAccId });
  res.json(data);
}
