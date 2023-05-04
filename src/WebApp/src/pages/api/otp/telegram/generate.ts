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

  const customerOtpSetting = await fetchServerWrapper.get(`${process.env.CUSTOMER_API}/customer/otp/${user?.custId}/${user?.subAccId}`);
  if (customerOtpSetting.isEnableTelegramOtp) {
    const url = `${process.env.CUSTOMER_API}/customer/otp/telegram/generate`;
    await fetchServerWrapper.post(url, { custId: user?.custId, subAccId: user?.subAccId });
    return res.json({ isSuccessful: true });
  } else {
    return res.json({ isSuccessful: false });
  }
}
