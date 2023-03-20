import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";
import { getIronSession } from "iron-session/edge";
import { sessionOptions } from "./lib/session";

export const middleware = async (req: NextRequest) => {
  const res = NextResponse.next();
  const session = await getIronSession(req, res, sessionOptions);

  // do anything with session here:
  const { user } = session;

  // like mutate user:
  // user.something = someOtherThing;
  // or:
  // session.user = someoneElse;

  // uncomment next line to commit changes:
  // await session.save();
  // or maybe you want to destroy session:
  // await session.destroy();

  console.log("from middleware", { user });

  console.log(req.url);

  // demo:
  if (user === undefined) {
    const { pathname, searchParams } = req.nextUrl;
    // unauthorized to see pages inside admin/
    return NextResponse.redirect(
      new URL(`/login?returnUrl=${pathname}${searchParams}`, req.url)
    ); // redirect to /unauthorized page
  }

  return res;
};

export const config = {
  matcher: "/newcustomer",
};
