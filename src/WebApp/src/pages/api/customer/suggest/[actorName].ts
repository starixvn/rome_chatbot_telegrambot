import { fetchWrapper } from "./../../../../lib/fetch-wrapper";
import type { NextApiRequest, NextApiResponse } from "next";
import { withIronSessionApiRoute } from "iron-session/next";
import { sessionOptions } from "src/lib/session";

export default withIronSessionApiRoute(suggestRouter, sessionOptions);

async function suggestRouter(req: NextApiRequest, res: NextApiResponse) {
  const user = req.session.user;
  if (user) {
    const actorName = req.query["actorName"];
    const body = {
      actorName: actorName,
      roleId: user?.customerLevel,
    };
    const url = `${process.env.CUSTOMER_API}/customer/suggest-username`;
    const suggestName = await fetchWrapper.post(url, body);
    res.status(200).json({ suggestName });
  } else {
    res.status(500).json({});
  }
}
