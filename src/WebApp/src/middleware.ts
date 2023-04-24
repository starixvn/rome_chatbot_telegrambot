import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";
import { getIronSession } from "iron-session/edge";
import { sessionOptions } from "./lib/session";

export const middleware = async (req: NextRequest) => {
  const res = NextResponse.next();
  const { pathname, searchParams, locale } = req.nextUrl;
  const system = searchParams.get("system") as string;
  searchParams.delete("system");

  const session = await getIronSession(req, res, sessionOptions);
  const { user, umInfo } = session;
  if (umInfo !== undefined && umInfo.isUnderMaintenanceTime) {
    return NextResponse.redirect(new URL(`um`, req.url));
  }

  if (user === undefined) {
    return NextResponse.redirect(new URL(`${locale}/login?returnUrl=${pathname}${searchParams}&system=${system}`, req.url)); // redirect to /unauthorized page
  }

  return res;
};

export const config = {
  matcher: ["/newcustomer"],
};
