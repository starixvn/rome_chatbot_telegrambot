import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";
import { getIronSession } from "iron-session/edge";
import { UMInfo, sessionOptions } from "./lib/session";
import { fetchWrapper } from "./lib/fetch-wrapper";

export const middleware = async (req: NextRequest) => {
  const res = NextResponse.next();
  const { pathname, searchParams, locale } = req.nextUrl;
  const umInfo = await getUMInfo(req);
  if (umInfo.isUnderMaintenanceTime) {
    return NextResponse.redirect(new URL("/um", req.url));
  }

  const session = await getIronSession(req, res, sessionOptions);
  const { user } = session;

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

const getUMInfo = async (req: NextRequest) => {
  const { searchParams } = req.nextUrl;
  const system = searchParams.get("system") as string;
  searchParams.delete("system");
  const url = new URL(`/api/um?system=${system}`, req.url);
  const umInfo = await fetchWrapper.get<UMInfo>(url.toString());
  return umInfo;
};
