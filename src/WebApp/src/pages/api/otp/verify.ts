import type { NextApiRequest, NextApiResponse } from "next";
import { withIronSessionApiRoute } from "iron-session/next";
import { sessionOptions } from "src/lib/session";
import { getClientIP } from "src/helpers/ip-helper";
import { fetchWrapper } from "src/lib/fetch-wrapper";

export default withIronSessionApiRoute(verifyRouter, sessionOptions);

async function verifyRouter(req: NextApiRequest, res: NextApiResponse) {
  if (req.method !== "POST") {
    res.status(405).send("Method not allowed");
    return;
  }

  const { user } = req.session;

  if (!user || user.isLoggedIn === false) {
    res.status(401).end();
    return;
  }

  const body = {
    custId: user?.custId,
    subAccId: user?.subAccId,
    otp: req.body.otp,
    language: user?.language,
    ip: getClientIP(req),
    isEnabledTelegramOTP: req.body.isEnableTelegramOtp,
  };
  const url = `${process.env.CUSTOMER_API}/customer/otp/verify`;
  const result = await fetchWrapper.post(url, body);

  res.status(200).json({ isSuccessful: result.isSuccessful });
}
