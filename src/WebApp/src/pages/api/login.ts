import { NextApiRequest, NextApiResponse } from "next";
import CryptoJS from "crypto-js";
import { withIronSessionApiRoute } from "iron-session/next";
import { sessionOptions } from "../../lib/session";

type TelegramBot = {
  botId: number;
  accessToken: string;
};

export default withIronSessionApiRoute(loginRoute, sessionOptions);

async function loginRoute(req: NextApiRequest, res: NextApiResponse) {
  const { telegramInitData } = req.body;
  try {
    const initData = new URLSearchParams(telegramInitData);
    const user = JSON.parse(initData.get("user") || "");
    const account = await getAccount(user.id);
    const botToken = await getBotToken(Number(account.botId));
    const isVerified = await verifyWebAppOpenByTelegram(botToken, initData);

    if (isVerified) {
      const data = {
        isLoggedIn: true,
        custId: account.custId,
        subAccId: account.subAccId,
        customerLevel: account.customerLevel,
        username: account.username,
        language: account.language,
      };
      req.session.user = data;
      await req.session.save();
    }
    res.json(req.session.user);
  } catch (error) {
    res.status(500).json({ message: (error as Error).message });
  }
}

const getBotToken = async (botId: number) => {
  const url = `${process.env.TELEGRAM_API_URL}/Bot`;
  const res = await fetch(url);
  const bots = await res.json();
  const bot: TelegramBot = bots.find((b: TelegramBot) => b.botId == botId);
  return bot.accessToken;
};

const getAccount = async (chatId: number) => {
  const url = `${process.env.TELEGRAM_API_URL}/Setting/${chatId}`;
  const res = await fetch(url);
  return await res.json();
};

const verifyWebAppOpenByTelegram = async (botToken: string, initData: URLSearchParams) => {
  const dataCheckString: string[] = [];
  initData.sort();

  initData.forEach((value, key) => {
    key !== "hash" && dataCheckString.push(`${key}=${value}`);
  });

  const secret = CryptoJS.HmacSHA256(botToken, "WebAppData");
  const checkHash = CryptoJS.HmacSHA256(dataCheckString.join("\n"), secret).toString(CryptoJS.enc.Hex);

  return initData.get("hash") === checkHash;
};
