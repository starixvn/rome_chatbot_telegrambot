import CryptoJS from "crypto-js";
import { withIronSessionApiRoute } from "iron-session/next";
import { SubAccountPermissions } from "src/constants/page-permission";
import { isDirectDownLine } from "src/constants/role-id";
import { formatThousandNumber } from "src/helpers/number";
import { fetchServerWrapper } from "src/lib/fetch-server";
import { sessionOptions } from "src/lib/session";
import withAuth, { checkPagePermission } from "src/lib/with-auth";

import { getClientIP } from "../../../helpers/ip-helper";
import { User } from "../../../lib/session";

import type { NextApiRequest, NextApiResponse } from "next";
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
  const { user, pageAccess } = req.session;

  if (user?.subAccId !== 0) {
    const hasPagePermission = checkPagePermission(pageAccess as string, SubAccountPermissions.Create);
    if (!hasPagePermission) {
      return res.status(401).end();
    }
  }

  const { credit, numberOfAccounts, sourceUserName, notifyTelegramMessage } = req.body;
  const sourceInfo = await fetchServerWrapper.get(`${process.env.CUSTOMER_API}/customer/${user?.custId}/${user?.subAccId}/downlines/${sourceUserName}`);
  const isExceedMaxAccount = process.env.MAX_ACCOUNTS && numberOfAccounts > process.env.MAX_ACCOUNTS;
  if (sourceInfo.custId <= 0 || !isDirectDownLine(sourceInfo.roleId, user?.customerLevel as number) || isExceedMaxAccount) {
    res.status(500).json({});
  }

  const siteDBGroup = await fetch(`${process.env.TELEGRAM_API_URL}/Setting/GetSiteDBGroup?custId=${sourceInfo.custId}`);
  const custDBGroupID = (await siteDBGroup.json()).custDBGroupID;
  const data = await copyCustomer(user as User, { ...req.body, sourceCustId: sourceInfo.custId, ip: getClientIP(req) }, await custDBGroupID);
  data.usernames.length > 0 &&
    (await notifySuccessCustomersCreated(user as User, { ...data, credit: credit }, numberOfAccounts - data.usernames.length, notifyTelegramMessage));
  res.json({ ...data, credit: credit, failAccounts: numberOfAccounts - data.usernames.length });
}
const copyCustomer = async (user: User, data: CopyCustomerData, custDBGroupID: number) => {
  const url = `${process.env.CUSTOMER_API}/customer/copy-multiple`;
  const rawPassword = CryptoJS.AES.decrypt(data.password, user.salt).toString(CryptoJS.enc.Utf8);
  const body = {
    actorId: user.custId,
    actorSubAccId: user.subAccId,
    uplineId: user.custId,
    custLevel: user.customerLevel - 1,
    actorUsername: user.username,
    userName: data.username,
    password: rawPassword,
    credit: data.credit,
    maxCredit: data.memberMaxCredit,
    sourceCustId: data.sourceCustId,
    numberOfAccounts: data.numberOfAccounts,
    site: user.site,
    isQRCode: false,
    language: user.language,
    ip: data.ip,
    systemId: 10,
    custDBGroupID: custDBGroupID,
  };
  return await fetchServerWrapper.post(url, body);
};

const notifySuccessCustomersCreated = async (user: User, data: any, failAccounts: number, translation: NotifyCopyCustomersTranslate) => {
  const newLineCharacter = "\u000a";
  const url = `${process.env.TELEGRAM_API_URL}/notification/success-customers-created`;
  const successMessage = `${translation.success.replace("{{no}}", data.usernames.length.toString())}${newLineCharacter}${
    translation.usernames
  }:  <code>${data.usernames.join(", ")}</code>${newLineCharacter}${translation.givenCreditPerAccount}:  ${formatThousandNumber(data.credit)}`;

  const failMessage =
    failAccounts > 0
      ? `${newLineCharacter}${newLineCharacter}${translation.fail.replace("{{failAccounts}}", failAccounts.toString())}${newLineCharacter}${
          translation.failReason
        }:  ${data.message}`
      : "";
  return await fetchServerWrapper.post(url, { custId: user.custId, subAccId: user.subAccId, message: `${successMessage}${failMessage}` });
};
