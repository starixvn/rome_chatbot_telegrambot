import { NextFetchEvent, NextResponse } from "next/server";
import type { NextRequest } from "next/server";
import { getIronSession } from "iron-session/edge";
import { sessionOptions } from "./lib/session";

export const middleware = async (req: NextRequest, event: NextFetchEvent) => {
  const res = NextResponse.next();
  const { pathname, searchParams, locale } = req.nextUrl;
  const session = await getIronSession(req, res, sessionOptions);
  const isMaintain = process.env.IS_UNDER_MAINTENANCE_TIME;
  if (isMaintain == "true") {
    session.umInfo = { from: process.env.MAINTENANCE_TIME_FROM as string, to: process.env.MAINTENANCE_TIME_TO as string, isUnderMaintenanceTime: true };
  } else {
    event.waitUntil(
      getUMInfo().then((json) => {
        session.umInfo = { from: json.from, to: json.to, isUnderMaintenanceTime: json.isUnderMaintenanceTime };
      })
    );
  }

  await session.save();
  const { user, umInfo } = session;

  const system = searchParams.get("system") as string;
  searchParams.delete("system");
  if (umInfo !== undefined && umInfo.isUnderMaintenanceTime) {
    return NextResponse.redirect(new URL(`/um?system=${system}&from=${umInfo.from}&to=${umInfo.to}`, req.url));
  }

  if (user === undefined) {
    if (pathname.startsWith("/api")) {
      return NextResponse.redirect(new URL("/unauthorized", req.url));
    } else {
      return NextResponse.redirect(new URL(`${locale}/login?returnUrl=${pathname}${searchParams}`, req.url)); // redirect to /unauthorized page
    }
  }

  return res;
};

export const config = {
  matcher: ["/newcustomer", "/api/security-code/:path*", "/api/otp/:path*", "/api/customer/:path*"],
};

async function getUMInfo() {
  const res = await fetch(`${process.env.UM_URL}/UnderMaintenance/get?siteId=${process.env.UM_SITE_ID}`);
  return await res.json();
}
