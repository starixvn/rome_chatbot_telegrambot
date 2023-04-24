import { withIronSessionApiRoute } from "iron-session/next";
import { NextApiRequest, NextApiResponse } from "next";
import { UMInfo, sessionOptions } from "../../lib/session";
import { fetchWrapper } from "./../../lib/fetch-wrapper";

export default withIronSessionApiRoute(umRoute, sessionOptions);

async function umRoute(req: NextApiRequest, res: NextApiResponse<UMInfo>) {
  const system = req.query["system"] as string;
  const umInfo = await getUmInfo();
  req.session.umInfo = { ...umInfo };
  req.session.isSpecialSite = system?.toLowerCase() == "789Y".toLowerCase();
  await req.session.save();
  res.json(req.session.umInfo as UMInfo);
}

const getUmInfo = async () => {
  const isMaintain = process.env.IS_UNDER_MAINTENANCE_TIME;
  if (isMaintain === "true") {
    return {
      from: process.env.MAINTENANCE_TIME_FROM,
      to: process.env.MAINTENANCE_TIME_TO,
      isUnderMaintenanceTime: true,
    };
  }

  const url = `${process.env.UM_URL}/UnderMaintenance/get?siteId=${process.env.UM_SITE_ID}`;
  return await fetchWrapper.get(url);
};
