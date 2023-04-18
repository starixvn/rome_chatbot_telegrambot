import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";
import { getIronSession } from "iron-session/edge";
import { sessionOptions } from "./lib/session";
import { fetchWrapper } from "./lib/fetch-wrapper";

export const middleware = async (req: NextRequest) => {
  if (!req.nextUrl.pathname.startsWith("/um")) {
    const umInfo = await getUmInfo();
    if (umInfo.isUnderMaintenanceTime) {
      const { locale } = req.nextUrl;
      const newUrl = new URL(`${locale}/um?from=${umInfo.from}&to=${umInfo.to}&`, req.url);
      return NextResponse.redirect(newUrl);
    }
  }

  if (req.nextUrl.pathname.startsWith("/newcustomer")) {
    const res = NextResponse.next();
    const session = await getIronSession(req, res, sessionOptions);
    const { user } = session;
    if (user === undefined) {
      const { pathname, searchParams, locale } = req.nextUrl;
      return NextResponse.redirect(new URL(`${locale}/login?returnUrl=${pathname}${searchParams}`, req.url)); // redirect to /unauthorized page
    }

    return res;
  }
};

export const config = {
  matcher: ["/((?!api|_next/static|_next/image|favicon.ico).*)"],
};

const getUmInfo = async () => {
  const isMaintain = process.env.IS_UNDER_MAINTENANCE_TIME;
  if (isMaintain === "true") {
    return {
      from: process.env.MAINTENANCE_TIME_FROM,
      to: process.env.MAINTENANCE_TIME_TO,
      isUnderMaintenanceTime: true,
      errorCode: 0,
    };
  }

  const url = `${process.env.UM_URL}/UnderMaintenance/Get`;
  return await fetchWrapper.get(url);
};
