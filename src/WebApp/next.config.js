/* eslint-disable @typescript-eslint/no-var-requires */
/** @type {import('next').NextConfig} */
const path = require("path");
const nextTranslate = require("next-translate-plugin");
const { withSentryConfig } = require("@sentry/nextjs");

module.exports = withSentryConfig(
  nextTranslate({
    reactStrictMode: true,
    output: "standalone",
    sassOptions: {
      includePaths: [path.join(__dirname, "styles")],
    },
    sentry: {
      // Use `hidden-source-map` rather than `source-map` as the Webpack `devtool`
      // for client-side builds. (This will be the default starting in
      // `@sentry/nextjs` version 8.0.0.) See
      // https://webpack.js.org/configuration/devtool/ and
      // https://docs.sentry.io/platforms/javascript/guides/nextjs/manual-setup/#use-hidden-source-map
      // for more information.
      hideSourceMaps: true,
      disableServerWebpackPlugin: true,
      disableClientWebpackPlugin: true,
    },
    assetPrefix: process.env.ASSET_PREFIX ?? undefined,
  })
);
