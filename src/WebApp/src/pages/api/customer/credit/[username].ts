import { fetchServerWrapper } from "src/lib/fetch-server";
import type { NextApiRequest, NextApiResponse } from "next";

export default async function handler(req: NextApiRequest, res: NextApiResponse) {
  const username = req.query["username"];
  const url = `${process.env.CUSTOMER_API}/customer/${username}/credit`;
  const { resStatus, data } = await fetchServerWrapper.get(url);
  res.status(resStatus).json(data);
}
