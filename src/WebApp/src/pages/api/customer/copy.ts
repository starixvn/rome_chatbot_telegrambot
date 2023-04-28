import { getClientIP } from "./../../../helpers/ip-helper";
import type { NextApiRequest, NextApiResponse } from "next";
import { withIronSessionApiRoute } from "iron-session/next";
import { sessionOptions } from "src/lib/session";
import { User } from "./../../../lib/session";
import { fetchServerWrapper } from "src/lib/fetch-server";
import withAuth from "src/lib/with-auth";

export default withIronSessionApiRoute(withAuth(copyRouter), sessionOptions);

type CopyCustomerData = {
  username: string;
  password: string;
  credit: number;
  memberMaxCredit: number;
  sourceCustId: number;
  sourceUserName: string;
  numberOfAccounts: number;
  ip: string;
};

async function copyRouter(req: NextApiRequest, res: NextApiResponse) {
  const user = req.session.user;

  const { credit, numberOfAccounts, sourceUserName } = req.body;
  const sourceInfo = await fetchServerWrapper.get(`${process.env.CUSTOMER_API}/customer/${user?.custId}/${user?.subAccId}/downlines/${sourceUserName}`);
  const data = await copyCustomer(user as User, { ...req.body, sourceCustId: sourceInfo.custId, ip: getClientIP(req) });
  res.json({ ...data, credit: credit, failAccounts: numberOfAccounts - data.usernames.length });
}

const copyCustomer = async (user: User, data: CopyCustomerData) => {
  const url = `${process.env.CUSTOMER_API}/customer/copy`;
  const hashedPassword = await hashPassword(data.password);

  const body = {
    actorId: user.custId,
    actorSubAccId: user.subAccId,
    actorSubAccName: "",
    actorUserLevel: user.customerLevel,
    userName: data.username,
    password: hashedPassword,
    credit: data.credit,
    maxCredit: data.memberMaxCredit,
    sourceCustId: data.sourceCustId,
    sourceUsername: data.sourceUserName,
    numberOfAccounts: data.numberOfAccounts,
    language: user.language,
    ip: data.ip,
    systemId: 10,
  };
  return await fetchServerWrapper.post(url, body);
};

const hashPassword = async (password: string): Promise<string> => {
  const url = `${process.env.HASH_PASSWORD_URL}`;
  const body = {
    password: password,
  };

  return await fetchServerWrapper.post(url, body);
};
