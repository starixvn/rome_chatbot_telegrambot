import { withIronSessionApiRoute } from "iron-session/next";
import { NextApiRequest, NextApiResponse } from "next";
import { sessionOptions } from "src/lib/session";
import * as Sentry from "@sentry/nextjs";

export default withIronSessionApiRoute(logoutRoute, sessionOptions);

function logoutRoute(req: NextApiRequest, res: NextApiResponse) {
  req.session.destroy();
  console.log("destroy");
  Sentry.captureMessage("destroy");
  res.send({ ok: true });
}
