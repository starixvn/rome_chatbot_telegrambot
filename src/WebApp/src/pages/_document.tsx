import { Html, Head, Main, NextScript } from "next/document";

export default function Document() {
  return (
    <Html lang="en">
      <Head>
        <script src="https://telegram.org/js/telegram-web-app.js" defer />
      </Head>
      <body>
        <div id="main" />
        <Main />
        <NextScript />
      </body>
    </Html>
  );
}
