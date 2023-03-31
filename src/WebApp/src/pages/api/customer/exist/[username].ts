import { fetchWrapper } from "./../../../../lib/fetch-wrapper";
import type { NextApiRequest, NextApiResponse } from "next";

export default async function handler(req: NextApiRequest, res: NextApiResponse) {
  const username = req.query["username"];
  const url = `${process.env.CUSTOMER_API}/customer/${username}/exist`;
  const isExist = await fetchWrapper.get(url);
  res.status(200).json({ isExist });
}
