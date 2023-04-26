import { NextApiHandler, NextApiRequest, NextApiResponse } from "next";

export default function withAuth(handler: NextApiHandler): NextApiHandler {
  return async (req: NextApiRequest, res: NextApiResponse) => {
    const { user } = req.session;

    return handler(req, res);
  };
}
