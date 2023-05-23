import { withIronSessionApiRoute } from "iron-session/next";
import { NextApiRequest, NextApiResponse } from "next";
import { getClientIP } from "src/helpers/ip-helper";
import { sessionOptions } from "src/lib/session";
import { fetchServerWrapper } from "src/lib/fetch-server";
import withAuth from "src/lib/with-auth";
import CryptoJS from "crypto-js";

const ACCOUNT_CLOCKED_CODE = 200;

export default withIronSessionApiRoute(withAuth(verifyRout), sessionOptions);

async function verifyRout(req: NextApiRequest, res: NextApiResponse) {
  if (req.method !== "POST") {
    res.status(405).send("Method not allowed");
    return;
  }

  const { user } = req.session;

  const body = {
    custId: user?.custId,
    subAccId: user?.subAccId,
    securityCode: CryptoJS.AES.decrypt(req.body.securityCode, user?.salt as string).toString(CryptoJS.enc.Utf8),
    language: user?.language,
    ip: getClientIP(req),
  };
  const url = `${process.env.CUSTOMER_API}/security-code/verify`;
  const data = await fetchServerWrapper.post(url, body);
  req.session.user && (req.session.user.isVerifyOtpOrSC = data.isSuccessful);
  await req.session.save();

  if (data.code === ACCOUNT_CLOCKED_CODE) {
    return res.status(403).json({ key: "AccountClocked" });
  }

  res.json(data);
}
