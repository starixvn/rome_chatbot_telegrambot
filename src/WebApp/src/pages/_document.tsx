import { Html, Head, Main, NextScript } from "next/document";

// const themeInitializerScript = `(function () {
//   document.body.dataset.theme = "light";
// })();`;

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
        {/* <script dangerouslySetInnerHTML={{ __html: themeInitializerScript }} /> */}
      </body>
    </Html>
  );
}
