import type { NextApiRequest, NextApiResponse } from "next";
import { withIronSessionApiRoute } from "iron-session/next";
import { sessionOptions } from "src/lib/session";

export default withIronSessionApiRoute(suggestRouter, sessionOptions);

async function suggestRouter(req: NextApiRequest, res: NextApiResponse) {
  const user = req.session.user;
  if (user) {
    const actorName = req.query["actorName"];
    const data = {
      actorName: actorName,
      custId: user?.custId,
      roleId: user?.customerLevel,
    };
    const url = `${process.env.CUSTOMER_API}/customer/suggest-username`;
    const result = await fetch(url, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(data),
    });
    const suggestName = await result.json();
    res.status(200).json({ suggestName });
  } else {
    res.status(500).json({});
  }
}
