import type { NextApiRequest, NextApiResponse } from "next";
import { withIronSessionApiRoute } from "iron-session/next";
import { fetchServerWrapper } from "src/lib/fetch-server";
import { sessionOptions } from "src/lib/session";
import withAuth from "src/lib/with-auth";

export default withIronSessionApiRoute(withAuth(searchRouter), sessionOptions);

async function searchRouter(req: NextApiRequest, res: NextApiResponse) {
  const user = req.session.user;
  const query = req.query["query"];
  const url = `${process.env.CUSTOMER_API}/customer/username-suggestion`;
  const data = await fetchServerWrapper.post(url, { actorId: user?.custId, query, directDownlines: true, excludeMember: true, excludeCashMember: true, isAccInfo: false });
  res.json(data);
}
