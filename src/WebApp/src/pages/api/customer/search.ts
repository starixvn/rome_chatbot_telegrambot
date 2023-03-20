import type { NextApiRequest, NextApiResponse } from "next";
import { withIronSessionApiRoute } from "iron-session/next";
import { sessionOptions } from "src/lib/session";

export default withIronSessionApiRoute(searchRouter, sessionOptions);

async function searchRouter(req: NextApiRequest, res: NextApiResponse) {
  const user = req.session.user;
  const query = req.query["query"];
  const url = `${process.env.CUSTOMER_API}/customer/search?custId=${user?.custId}&query=${query}`;
  const result = await fetch(url);
  const customers = await result.json();
  res.status(200).json(customers);
}
