import type { NextApiRequest, NextApiResponse } from "next";
import { z } from "zod";

const querySchema = z.object({
  username: z.string(),
});

export default async function handler(req: NextApiRequest, res: NextApiResponse) {
  const { username } = querySchema.parse(req.query);
  const url = `${process.env.CUSTOMER_API}/customer/${username}/exist`;
  const result = await fetch(url);
  const isExist = await result.json();
  res.status(200).json({ isExist });
}
