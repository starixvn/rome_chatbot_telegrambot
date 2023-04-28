import { NextFetchEvent, NextResponse } from "next/server";
import type { NextRequest } from "next/server";
import { getIronSession } from "iron-session/edge";
import { sessionOptions } from "./lib/session";
import { getCustomerStatus, getUmInfo } from "./lib/with-auth";

export const middleware = async (req: NextRequest, event: NextFetchEvent) => {
  const res = NextResponse.next();
  const { pathname, searchParams, locale } = req.nextUrl;

  const umInfo = await getUmInfo();
  const system = searchParams.get("system") as string;
  searchParams.delete("system");
  const isSpecialSite = (system as string)?.toLowerCase() == "789Y".toLowerCase();
  res.cookies.set("isSpecialSite", String(isSpecialSite));
  if (umInfo !== undefined && umInfo.isUnderMaintenanceTime) {
    return NextResponse.redirect(new URL(`/um?isSpecialSite=${isSpecialSite}&from=${umInfo.from}&to=${umInfo.to}`, req.url));
  }

  const session = await getIronSession(req, res, sessionOptions);
  const { user } = session;

  if (user === undefined) {
    return NextResponse.redirect(new URL(`${locale}/login?returnUrl=${pathname}${searchParams}`, req.url)); // redirect to /unauthorized page
  }

  const customerStatus = await getCustomerStatus(user);
  if (!customerStatus.isActive || customerStatus.isProtectedMode || customerStatus.isLocked) {
    return NextResponse.redirect(
      new URL(`/kickout?isActive=${customerStatus.isActive}&isProtectedMode=${customerStatus.isProtectedMode}&isLocked=${customerStatus.isLocked}`, req.url)
    );
  }

  return res;
};

export const config = {
  matcher: ["/newcustomer"],
};
