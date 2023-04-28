import type { NextApiRequest, NextApiResponse } from "next";
import { withIronSessionApiRoute } from "iron-session/next";
import { sessionOptions } from "src/lib/session";
import { getClientIP } from "src/helpers/ip-helper";
import { fetchServerWrapper } from "src/lib/fetch-server";
import withAuth from "src/lib/with-auth";

const ACCOUNT_CLOCKED_CODE = 200;

export default withIronSessionApiRoute(withAuth(verifyRouter), sessionOptions);

async function verifyRouter(req: NextApiRequest, res: NextApiResponse) {
  if (req.method !== "POST") {
    res.status(405).send("Method not allowed");
    return;
  }

  const { user } = req.session;

  const postData = {
    custId: user?.custId,
    subAccId: user?.subAccId,
    otp: req.body.otp,
    language: user?.language,
    ip: getClientIP(req),
    isEnabledTelegramOTP: req.body.isEnableTelegramOtp,
  };
  const url = `${process.env.CUSTOMER_API}/customer/otp/verify`;
  const data = await fetchServerWrapper.post(url, postData);
  req.session.user && (req.session.user.isVerifyOtpOrSC = data.isSuccessful);
  await req.session.save();

  if (data.code === ACCOUNT_CLOCKED_CODE) {
    return res.status(403).json({ isActive: true, isLocked: true, isProtectedMode: false });
  }
  res.json({ isSuccessful: data.isSuccessful });
}
