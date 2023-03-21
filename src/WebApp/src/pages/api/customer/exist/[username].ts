import type { NextApiRequest, NextApiResponse } from "next";

export default async function handler(req: NextApiRequest, res: NextApiResponse) {
  const username = req.query["username"];
  const url = `${process.env.CUSTOMER_API}/customer/${username}/exist`;
  const result = await fetch(url);
  const isExist = await result.json();
  res.status(200).json({ isExist });
}
