import type { NextApiRequest, NextApiResponse } from "next";
import { withIronSessionApiRoute } from "iron-session/next";
import { sessionOptions } from "src/lib/session";
import { User } from "./../../../lib/session";

export default withIronSessionApiRoute(copyRouter, sessionOptions);

async function copyRouter(req: NextApiRequest, res: NextApiResponse) {
  const user = req.session.user;
  if (user?.isLoggedIn) {
    const { credit, numberOfAccounts, sourceUserName } = req.body;
    const sourceInfo = await getDownLineInfo(user, sourceUserName);
    const data = await copyCustomer(user, { ...req.body, sourceCustId: sourceInfo.custId });
    res.status(200).json({ ...data, credit: credit, failAccounts: numberOfAccounts - data.usernames.length });
  } else {
    res.status(500).json({});
  }
}

const getDownLineInfo = async (user: User, sourceUserName: string) => {
  const url = `${process.env.CUSTOMER_API}/customer/${user.custId}/${user.subAccId}/downlines/${sourceUserName}`;
  const result = await fetch(url);
  return await result.json();
};

const copyCustomer = async (user: User, data: any) => {
  const url = `${process.env.CUSTOMER_API}/customer/copy`;
  const reqData = {
    actorId: user.custId,
    actorSubAccId: user.subAccId,
    actorSubAccName: "",
    actorUserLevel: user.customerLevel,
    userName: data.username,
    password: data.password,
    credit: data.credit,
    maxCredit: data.memberMaxCredit,
    sourceCustId: data.sourceCustId,
    sourceUsername: data.sourceUserName,
    numberOfAccounts: data.numberOfAccounts,
    language: user.language,
    ip: "127.0.0.1",
    systemId: 10,
  };

  const result = await fetch(url, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(reqData),
  });

  return await result.json();
};
