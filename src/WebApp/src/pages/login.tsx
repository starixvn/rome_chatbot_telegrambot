import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import { CustomWindow } from "src/types/custom.window";

declare let window: CustomWindow;

export default function Login() {
  const [isLoading] = useState(false);
  const router = useRouter();

  useEffect(() => {
    const signIn = async () => {
      const telegramInitData = 
        "query_id=AAH_jLdoAAAAAP-Mt2iuwqry&user=%7B%22id%22%3A1756859647%2C%22first_name%22%3A%22Issac%22%2C%22last_name%22%3A%22%22%2C%22username%22%3A%22issacnexcel%22%2C%22language_code%22%3A%22en%22%7D&auth_date=1679043542&hash=4259dbfd6a4980e73f69d224171c150b1fb726020cb4f67572fc189474cd3476";
      
      if (!telegramInitData) {
        router.push("/unauthorized");
      }

      const { returnUrl } = router.query;

      const res = await fetch("/api/login", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ telegramInitData }),
      });
      const authData = await res.json();
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
