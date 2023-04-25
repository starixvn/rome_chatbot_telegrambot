import ErrorComponent from "@features/CommonPage/Error";
import { NextPageContext } from "next";
import NextErrorComponent, { ErrorProps } from "next/error";
import * as Sentry from "@sentry/nextjs";

const ErrorPage = (props: ErrorProps) => {
  return <ErrorComponent statusCode={props.statusCode} />;
};

ErrorPage.getInitialProps = async (contextData: NextPageContext) => {
  const initialProps = await NextErrorComponent.getInitialProps(contextData);

  const errorInitialProps = {
    ...initialProps,
    hasGetInitialPropsRun: true,
  };

  contextData.err
    ? Sentry.captureException(contextData.err)
    : Sentry.captureException(new Error(`_error.js getInitialProps missing data at path: ${contextData.asPath}`));

  await Sentry.flush(2000);

  return errorInitialProps;
};

export default ErrorPage;
