import CryptoJS from "crypto-js";
import { withIronSessionApiRoute } from "iron-session/next";
import { NextApiRequest, NextApiResponse } from "next";
import { fetchServerWrapper } from "src/lib/fetch-server";

import * as Sentry from "@sentry/nextjs";

import { sessionOptions, User } from "../../lib/session";

type TelegramBot = {
  botId: number;
  accessToken: string;
};

export default withIronSessionApiRoute(loginRoute, sessionOptions);

async function loginRoute(req: NextApiRequest, res: NextApiResponse) {
  const { telegramInitData } = req.body;
  const { user } = req.session;
  try {
    const initData = new URLSearchParams(telegramInitData);
    const tgHash = initData.get("hash") as string;
    const tgUser = JSON.parse(initData.get("user") || "");
    if (user?.isLoggedIn && user.hash === tgHash) {
      req.session.user = {
        isLoggedIn: true,
        custId: user.custId,
        subAccId: user.subAccId,
        tgChatId: tgUser.id,
        customerLevel: user.customerLevel,
        username: user.username,
        language: user.language,
        site: user.site,
        isVerifyOtpOrSC: user.isVerifyOtpOrSC,
        hash: user.hash,
        salt: CryptoJS.lib.WordArray.random(32).toString(CryptoJS.enc.Hex),
      };
      req.session.reChecking = true;
      await req.session.save();
      return res.status(200).json(req.session.user);
    }

    const account = await fetchServerWrapper.get(`${process.env.TELEGRAM_API_URL}/Setting/${tgUser.id}`);
    const botToken = await getBotToken(Number(account.botId));
    const isVerified = await verifyWebAppOpenByTelegram(botToken, initData);
    if (isVerified) {
      req.session.user = {
        isLoggedIn: true,
        custId: account.custId,
        subAccId: account.subAccId,
        tgChatId: tgUser.id,
        customerLevel: account.customerLevel,
        username: account.username,
        language: account.language,
        site: account.site,
        isVerifyOtpOrSC: false,
        hash: tgHash,
        salt: CryptoJS.lib.WordArray.random(32).toString(CryptoJS.enc.Hex),
      };
      req.session.isSpecialSite = account.system.toLowerCase() == "789Y".toLowerCase();
      req.session.reChecking = true;
      await req.session.save();
      res.status(200).json(req.session.user);
    } else {
      req.session.destroy();
      await req.session.save();
      res.status(200).json({ isLoggedIn: false });
    }
  } catch (error) {
    req.session.destroy();
    await req.session.save();
    Sentry.captureException(error);
    res.status(500).json({ message: (error as Error).message });
  }
}

const getBotToken = async (botId: number) => {
  const bots = await fetchServerWrapper.get(`${process.env.TELEGRAM_API_URL}/Bot`);
  const bot: TelegramBot = bots.find((b: TelegramBot) => b.botId == botId);
  return bot.accessToken;
};

const verifyWebAppOpenByTelegram = async (botToken: string, initData: URLSearchParams) => {
  const dataCheckString: string[] = [];
  const authDate = Number(initData.get("auth_date"));
  initData.sort();

  initData.forEach((value, key) => {
    key !== "hash" && dataCheckString.push(`${key}=${value}`);
  });

  const secret = CryptoJS.HmacSHA256(botToken, "WebAppData");
  const checkHash = CryptoJS.HmacSHA256(dataCheckString.join("\n"), secret).toString(CryptoJS.enc.Hex);

  return initData.get("hash") === checkHash && Date.now() / 1000 - authDate <= Number(process.env.EXPIRE_AUTH_DATE_TIME);
};
