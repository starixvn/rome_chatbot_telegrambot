import { fetchServerWrapper } from "src/lib/fetch-server";
import type { NextApiRequest, NextApiResponse } from "next";
import { withIronSessionApiRoute } from "iron-session/next";
import { sessionOptions } from "src/lib/session";
import withAuth from "src/lib/with-auth";

export default withIronSessionApiRoute(withAuth(existRouter), sessionOptions);

async function existRouter(req: NextApiRequest, res: NextApiResponse) {
  const username = req.query["username"];
  const url = `${process.env.CUSTOMER_API}/customer/${username}/exist`;
  const data = await fetchServerWrapper.get(url);
  res.json(data);
}
