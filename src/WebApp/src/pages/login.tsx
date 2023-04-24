import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import { fetchWrapper } from "src/lib/fetch-wrapper";
import { CustomWindow } from "src/types/custom.window";
import Loading from "@components/Loading";
import { UMInfo } from "src/lib/session";

declare let window: CustomWindow;

export default function Login() {
  const [isLoading] = useState(true);
  const router = useRouter();

  useEffect(() => {
    const signIn = async () => {
      const telegramInitData = window.Telegram.WebApp.initData || null;

      if (!telegramInitData) {
        router.push("/unauthorized");
      }
      const { returnUrl, system } = router.query;

      const umInfo = await fetchWrapper.get<UMInfo>(`/api/um?system=${system}`);
      if (umInfo.isUnderMaintenanceTime) {
        router.push(`/um`);
        return;
      }

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
    return <Loading />;
  }
}
