import type { NextApiRequest, NextApiResponse } from "next";

export default async function handler(req: NextApiRequest, res: NextApiResponse) {
  const username = req.query["username"];
  const url = `${process.env.CUSTOMER_API}/${username}/credit`;
  const result = await fetch(url);
  res.status(200).json(await result.json());
}
