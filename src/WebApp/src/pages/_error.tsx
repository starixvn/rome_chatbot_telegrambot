// import { NextPageContext } from "next";

// interface Props {
//   statusCode?: number;
// }

// function Error({ statusCode = 404 }: Props) {
//   return (
//     <p>
//       {statusCode
//         ? `An error ${statusCode} occurred on server`
//         : "An error occurred on client"}
//     </p>
//   );
// }

// // Error.getInitialProps = ({ res, err }: NextPageContext) => {
// //   const statusCode = res ? res.statusCode : err ? err.statusCode : 404;
// //   return { statusCode };
// // };

// export default Error;

import * as Sentry from '@sentry/nextjs'
import { NextPageContext } from 'next'
import NextErrorComponent, { ErrorProps } from 'next/error'

const CustomErrorComponent = (props: ErrorProps) => (
  <NextErrorComponent statusCode={props.statusCode} />
)

CustomErrorComponent.getInitialProps = async (contextData: NextPageContext) => {
  // In case this is running in a serverless function, await this in order to give Sentry
  // time to send the error before the lambda exits
  await Sentry.captureUnderscoreErrorException(contextData)

  // This will contain the status code of the response
  return NextErrorComponent.getInitialProps(contextData)
}

export default CustomErrorComponent
