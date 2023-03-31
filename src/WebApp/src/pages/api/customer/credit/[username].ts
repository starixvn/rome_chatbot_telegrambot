import { fetchWrapper } from "./../../../../lib/fetch-wrapper";
import type { NextApiRequest, NextApiResponse } from "next";

export default async function handler(req: NextApiRequest, res: NextApiResponse) {
  const username = req.query["username"];
  const url = `${process.env.CUSTOMER_API}/customer/${username}/credit`;
  res.status(200).json(await fetchWrapper.get(url));
}
