import { getClientIP } from "./../../../helpers/ip-helper";
import type { NextApiRequest, NextApiResponse } from "next";
import { withIronSessionApiRoute } from "iron-session/next";
import { sessionOptions } from "src/lib/session";
import { User } from "./../../../lib/session";
import { fetchServerWrapper } from "src/lib/fetch-server";
import withAuth, { checkIsLinkTelegramBot } from "src/lib/with-auth";
import { isDirectDownLine } from "src/constants/role-id";
import CryptoJS from "crypto-js";

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

type NotifyCopyCustomersTranslate = {
  success: string;
  usernames: string;
  givenCreditPerAccount: string;
  fail: string;
  failReason: string;
};

async function copyRouter(req: NextApiRequest, res: NextApiResponse) {
  const user = req.session.user;

  const isLinkBot = await checkIsLinkTelegramBot(Number(user?.tgChatId));
  if (isLinkBot === undefined) {
    return res.status(403).json({ key: "isUnLinkMessage" });
  }

  const { credit, numberOfAccounts, sourceUserName, notifyTelegramMessage } = req.body;
  const sourceInfo = await fetchServerWrapper.get(`${process.env.CUSTOMER_API}/customer/${user?.custId}/${user?.subAccId}/downlines/${sourceUserName}`);
  const isExceedMaxAccount = process.env.MAX_ACCOUNTS && numberOfAccounts > process.env.MAX_ACCOUNTS;
  if (sourceInfo.custId <= 0 || !isDirectDownLine(sourceInfo.roleId, user?.customerLevel as number) || isExceedMaxAccount) {
    res.status(500).json({});
  }
  const data = await copyCustomer(user as User, { ...req.body, sourceCustId: sourceInfo.custId, ip: getClientIP(req) });
  data.usernames.length > 0 &&
    (await notifySuccessCustomersCreated(user as User, { ...data, credit: credit }, numberOfAccounts - data.usernames.length, notifyTelegramMessage));
  res.json({ ...data, credit: credit, failAccounts: numberOfAccounts - data.usernames.length });
}

const copyCustomer = async (user: User, data: CopyCustomerData) => {
  const url = `${process.env.CUSTOMER_API}/customer/copy`;
  const hashedPassword = await hashPassword(CryptoJS.AES.decrypt(data.password, user.salt).toString(CryptoJS.enc.Utf8));

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

const notifySuccessCustomersCreated = async (user: User, data: any, failAccounts: number, translation: NotifyCopyCustomersTranslate) => {
  const newLineCharacter = "\u000a";
  const url = `${process.env.TELEGRAM_API_URL}/notification/success-customers-created`;
  const successMessage = `${translation.success.replace("{{no}}", data.usernames.length.toString())}${newLineCharacter}${
    translation.usernames
  }:  <code>${data.usernames.join(", ")}</code>${newLineCharacter}${translation.givenCreditPerAccount}:  ${data.credit}`;

  const failMessage =
    failAccounts > 0
      ? `${newLineCharacter}${newLineCharacter}${translation.fail.replace("{{failAccounts}}", failAccounts.toString())}${newLineCharacter}${
          translation.failReason
        }:  ${data.message}`
      : "";
  return await fetchServerWrapper.post(url, { custId: user.custId, subAccId: user.subAccId, message: `${successMessage}${failMessage}` });
};
