import React, { useState } from "react";
import { withIronSessionSsr } from "iron-session/next";
import { sessionOptions } from "src/lib/session";
import Layout from "@components/Layout";
import useTranslation from "next-translate/useTranslation";
import { CopyCustomer } from "@features/CopyCustomer";
import SecurityCodeForm from "@components/SecurityCodeForm";
import VerifyOtp from "@components/OtpForm";
import { fetchWrapper } from "src/lib/fetch-wrapper";
import { Master } from "src/constants/role-id";
import * as Sentry from "@sentry/nextjs";

type CustomerOtpSetting = {
  isEnableSophosOtp: boolean;
  isEnableTelegramOtp: boolean;
};

type Props = {
  customerOtp: CustomerOtpSetting;
  minCredit: number;
  maxAccounts: number;
  actorName: string;
  isMasterLevel: boolean;
  customerLevel: number;
};

const NewCustomer = ({ customerOtp, customerLevel, ...props }: Props) => {
  const { t } = useTranslation("newcustomer");
  const [isSecurityCodeVerified, setIsSecurityCodeVerified] = useState(false);
  const onVerifySecurityCodeSuccess = () => {
    setIsSecurityCodeVerified(true);
  };

  if (isSecurityCodeVerified) {
    return (
      <Layout title={t("header")} roleId={customerLevel}>
        <CopyCustomer {...props} />
      </Layout>
    );
  }

  return (
    <Layout title={t("header")} roleId={customerLevel}>
      {customerOtp.isEnableSophosOtp || customerOtp.isEnableTelegramOtp ? (
        <VerifyOtp {...customerOtp} actorName={props.actorName} onVerifySuccess={onVerifySecurityCodeSuccess} />
      ) : (
        <SecurityCodeForm username={props.actorName} onVerifySuccess={onVerifySecurityCodeSuccess} />
      )}
    </Layout>
  );
};

export const getServerSideProps = withIronSessionSsr(async function ({ req, res }) {
  const user = req.session.user;

  if (user === undefined) {
    Sentry.captureMessage("User undefined -> Unauthorized");
    res.setHeader("location", "/unauthorized");
    res.statusCode = 302;
    res.end();
    return {
      props: {
        customerOtp: {
          isEnableSophosOtp: false,
          isEnableTelegramOtp: false,
        },
        minCredit: 0,
        maxAccounts: process.env.MAX_ACCOUNTS,
        actorName: "",
        customerLevel: 0,
        isMasterLevel: false,
      },
    };
  }

  const settingOtp = await fetchWrapper.get<CustomerOtpSetting>(`${process.env.CUSTOMER_API}/customer/otp/${user.custId}/${user.subAccId}`);

  return {
    props: {
      customerOtp: settingOtp,
      minCredit: 0,
      maxAccounts: process.env.MAX_ACCOUNTS,
      actorName: user.username,
      customerLevel: user.customerLevel,
      isMasterLevel: user.customerLevel === Master,
    },
  };
}, sessionOptions);

export default NewCustomer;
