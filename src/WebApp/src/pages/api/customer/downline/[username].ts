import type { NextApiRequest, NextApiResponse } from "next";
import { withIronSessionApiRoute } from "iron-session/next";
import { isDirectDownLine } from "src/constants/role-id";
import { ServiceType } from "src/constants/service-type";
import { fetchServerWrapper } from "src/lib/fetch-server";
import { sessionOptions } from "src/lib/session";
import withAuth from "src/lib/with-auth";

export default withIronSessionApiRoute(withAuth(downLineRouter), sessionOptions);

async function downLineRouter(req: NextApiRequest, res: NextApiResponse) {
  const user = req.session.user;
  const username = req.query["username"];

  const checkDownLineUrl = `${process.env.CUSTOMER_API}/customer/${user?.custId}/${user?.subAccId}/downlines/${username}`;
  const downLineInfo = await fetchServerWrapper.get(checkDownLineUrl);

  if (downLineInfo && downLineInfo.custId > 0) {
    const checkStatusUrl = `${process.env.CUSTOMER_API}/customer/${downLineInfo.custId}/IsEnabled`;
    const isEnable = await fetchServerWrapper.get(checkStatusUrl);
    const serviceType: number = await fetchServerWrapper.get(`${process.env.CUSTOMER_API}/customer/${downLineInfo.custId}/cash-service-status`);
    res.json({ isDownLineUser: isDirectDownLine(downLineInfo.roleId, user?.customerLevel as number) && serviceType !== ServiceType.Cash, isEnable });
  } else {
    res.json({ isDownLineUser: false, isEnable: false });
  }
}
