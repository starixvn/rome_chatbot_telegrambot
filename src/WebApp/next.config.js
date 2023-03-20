/** @type {import('next').NextConfig} */
const path = require("path");
const nextTranslate = require("next-translate-plugin");

const isProd = process.env.NODE_ENV === "production";

module.exports = nextTranslate({
  reactStrictMode: true,
  output: "standalone",
  assetPrefix: isProd ? "/customer" : undefined,
  sassOptions: {
    includePaths: [path.join(__dirname, "styles")],
  },
});
