import { NextFetchEvent, NextResponse } from "next/server";
import type { NextRequest } from "next/server";
import { getIronSession } from "iron-session/edge";
import { sessionOptions } from "./lib/session";
import { PagePermission, Pages, checkIsLinkTelegramBot, checkPagePermission, getCustomerStatus, getUmInfo } from "./lib/with-auth";

export const middleware = async (req: NextRequest, event: NextFetchEvent) => {
  const res = NextResponse.next();
  const { pathname, searchParams, locale } = req.nextUrl;

  const umInfo = await getUmInfo();
  const system = searchParams.get("system") as string;
  searchParams.delete("system");
  const isSpecialSite = system?.toLowerCase() == "789Y".toLowerCase();
  if (umInfo !== undefined && umInfo.isUnderMaintenanceTime) {
    return NextResponse.redirect(new URL(`/um?isSpecialSite=${isSpecialSite}&from=${umInfo.from}&to=${umInfo.to}`, req.url));
  }

  const session = await getIronSession(req, res, sessionOptions);
  const { user, reChecking } = session;

  if (user === undefined || !reChecking || user.hash === undefined || user.tgChatId === undefined) {
    return NextResponse.redirect(new URL(`${locale}/login?returnUrl=${pathname}${searchParams}`, req.url)); // redirect to /unauthorized page
  }

  const isLinkBot = await checkIsLinkTelegramBot(user.tgChatId);
  if (!isLinkBot) {
    return NextResponse.redirect(new URL(`/unauthorized`, req.url));
  }

  const customerStatus = await getCustomerStatus(user);
  if (customerStatus.custId === undefined || !customerStatus.isActive || customerStatus.isProtectedMode || customerStatus.isLocked) {
    session.pageAccess = customerStatus.pageAccess;
    const key = !customerStatus.isActive ? "AccountClosed" : customerStatus.isLocked ? "AccountClocked" : "AccountProtected";
    return NextResponse.redirect(new URL(`/kickout?key=${key}`, req.url));
  }

  if (user.subAccId !== 0) {
    const hasPagePermission = checkPagePermission(customerStatus.pageAccess, PagePermission[new URL(req.url).pathname as Pages]);
    if (!hasPagePermission) {
      return NextResponse.redirect(new URL(`/unauthorized`, req.url));
    }
  }

  session.reChecking = false;
  await session.save();
  return res;
};

export const config = {
  matcher: ["/newcustomer"],
};
