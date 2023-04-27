import { NextFetchEvent, NextResponse } from "next/server";
import type { NextRequest } from "next/server";
import { getIronSession } from "iron-session/edge";
import { User, sessionOptions } from "./lib/session";

export const middleware = async (req: NextRequest, event: NextFetchEvent) => {
  const res = NextResponse.next();
  const { pathname, searchParams, locale } = req.nextUrl;

  const umInfo = await getUmInfo();
  const system = searchParams.get("system") as string;
  searchParams.delete("system");
  if (umInfo !== undefined && umInfo.isUnderMaintenanceTime) {
    return NextResponse.redirect(new URL(`/um?system=${system}&from=${umInfo.from}&to=${umInfo.to}`, req.url));
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

  const customerStatus = await getCustomerStatus(user);
  if (!customerStatus.isActive || customerStatus.isProtectedMode || customerStatus.isLocked) {
    return NextResponse.redirect(
      new URL(`/lock?isActive=${customerStatus.isActive}&isProtectedMode=${customerStatus.isProtectedMode}&isLocked=${customerStatus.isLocked}`, req.url)
    );
  }

  return res;
};

export const config = {
  matcher: ["/newcustomer", "/api/security-code/:path*", "/api/otp/:path*", "/api/customer/:path*"],
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
  const res = await fetch(url);
  return await res.json();
};

const getCustomerStatus = async (user: User) => {
  const res = await fetch(`${process.env.ALPHA_API}/customer/GetCustomerStatus?custId=${user.custId}&subAccId=${user.subAccId}`);
  return await res.json();
};
