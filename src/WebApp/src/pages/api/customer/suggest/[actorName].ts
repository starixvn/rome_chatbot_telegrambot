import type { NextApiRequest, NextApiResponse } from "next";
import { withIronSessionApiRoute } from "iron-session/next";
import { sessionOptions } from "src/lib/session";
import { fetchServerWrapper } from "src/lib/fetch-server";

export default withIronSessionApiRoute(suggestRouter, sessionOptions);

async function suggestRouter(req: NextApiRequest, res: NextApiResponse) {
  const user = req.session.user;
  const actorName = req.query["actorName"];
  const body = {
    actorName: actorName,
    roleId: user?.customerLevel,
  };
  const url = `${process.env.CUSTOMER_API}/customer/suggest-username`;
  const { resStatus, data } = await fetchServerWrapper.post(url, body);
  res.status(resStatus).json(data);
}
