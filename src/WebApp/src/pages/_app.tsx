import type { AppProps } from "next/app";
import "@styles/globals.scss";
import { useEffect } from "react";
import { CustomWindow } from "src/types/custom.window";

declare let window: CustomWindow;

export default function App({ Component, pageProps }: AppProps) {
  useEffect(() => {
    const handleBeforeUnload = (event: BeforeUnloadEvent) => {
      event.preventDefault();
      fetch("/api/logout", { method: "POST" });

      return true;
    };

    const handlePopupClosed = () => {
      fetch("/api/logout", { method: "POST" });

      return true;
    };

    window.addEventListener("beforeunload", handleBeforeUnload);
    window.Telegram.WebApp.onEvent("popupClosed", handlePopupClosed);

    return () => {
      window.removeEventListener("beforeunload", handleBeforeUnload);
    };
  }, []);

  return <Component {...pageProps} />;
}
