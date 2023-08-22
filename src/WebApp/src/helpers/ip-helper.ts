import { NextApiRequest } from "next";

export const getClientIP = (req: NextApiRequest) => {
  let ip = req.headers["x-actual-ip"] as string;

  const forwardedFor = req.headers["x-forwarded-for"] as string;
  if (!ip && forwardedFor) {
    ip = forwardedFor?.split(",").at(0) ?? "Unknown";
  }

  return ip ?? "127.0.0.1";
};
