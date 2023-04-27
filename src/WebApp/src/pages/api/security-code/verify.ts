import { withIronSessionApiRoute } from "iron-session/next";
import { NextApiRequest, NextApiResponse } from "next";
import { getClientIP } from "src/helpers/ip-helper";
import { sessionOptions } from "src/lib/session";
import { fetchServerWrapper } from "src/lib/fetch-server";

export default withIronSessionApiRoute(verifyRout, sessionOptions);

async function verifyRout(req: NextApiRequest, res: NextApiResponse) {
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
    securityCode: req.body.securityCode,
    language: user?.language,
    ip: getClientIP(req),
  };
  const url = `${process.env.CUSTOMER_API}/security-code/verify`;
  const { resStatus, data } = await fetchServerWrapper.post(url, body);
  req.session.user && (req.session.user.isVerifyOtpOrSC = data.isSuccessful);
  await req.session.save();
  res.status(resStatus).json(data);
}
