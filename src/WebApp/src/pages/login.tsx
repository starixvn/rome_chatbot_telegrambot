import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import { fetchWrapper } from "src/lib/fetch-wrapper";
import { CustomWindow } from "src/types/custom.window";

declare let window: CustomWindow;

export default function Login() {
  const [isLoading] = useState(false);
  const router = useRouter();

  useEffect(() => {
    const signIn = async () => {
      const telegramInitData = window.Telegram.WebApp.initData || null;

      if (!telegramInitData) {
        router.push("/unauthorized");
      }

      const { returnUrl } = router.query;
      const authData = await fetchWrapper.post("/api/login", { telegramInitData });
      if (authData.isLoggedIn) {
        router.push(returnUrl as string);
      } else {
        router.push("/unauthorized");
      }
    };

    if (router.isReady) {
      signIn();
    }
  }, [router]);

  if (isLoading) {
    return "Loading...";
  }
}
