import React, { useState } from "react";
import useTranslation from "next-translate/useTranslation";
import { withIronSessionSsr } from "iron-session/next";
import { User, sessionOptions } from "src/lib/session";
import { fetchWrapper } from "src/lib/fetch-client";
import { CopyCustomer } from "@features/CopyCustomer";
import Layout from "@components/Layout";
import SecurityCodeForm from "@components/SecurityCodeForm";
import VerifyOtp from "@components/OtpForm";
import * as Sentry from "@sentry/nextjs";

type CustomerOtpSetting = {
  isEnableSophosOtp: boolean;
  isEnableTelegramOtp: boolean;
};

type Props = {
  minCredit: number;
  maxAccounts: number;
  user: User;
  customerOtpSetting: CustomerOtpSetting;
  isSpecialSite: boolean;
};

const NewCustomer = ({ user, customerOtpSetting, isSpecialSite, ...props }: Props) => {
  const { t } = useTranslation("newcustomer");
  const [isSecurityCodeVerified, setIsSecurityCodeVerified] = useState(user.isVerifyOtpOrSC);
  const onVerifySecurityCodeSuccess = () => {
    setIsSecurityCodeVerified(true);
  };

  if (isSecurityCodeVerified) {
    return (
      <Layout title={t("header")} roleId={user.customerLevel} isSpecialSite={isSpecialSite}>
        <CopyCustomer user={user} {...props} />
      </Layout>
    );
  }

  return (
    <Layout title={t("header")} roleId={user.customerLevel} isSpecialSite={isSpecialSite}>
      {customerOtpSetting.isEnableSophosOtp || customerOtpSetting.isEnableTelegramOtp ? (
        <VerifyOtp {...customerOtpSetting} username={user.username} onVerifySuccess={onVerifySecurityCodeSuccess} />
      ) : (
        <SecurityCodeForm username={user.username} salt={user.salt} onVerifySuccess={onVerifySecurityCodeSuccess} />
      )}
    </Layout>
  );
};

export const getServerSideProps = withIronSessionSsr(async function ({ req, res }) {
  const user = req.session.user;

  Sentry.captureMessage(`${new Date().getMinutes()}:${new Date().getMilliseconds()}: custId: ${user?.custId} -> salt: ${user?.salt} -> hash: ${user?.hash}`);

  const customerOtpSetting = await fetchWrapper.get<CustomerOtpSetting>(`${process.env.CUSTOMER_API}/customer/otp/${user?.custId}/${user?.subAccId}`);
  return {
    props: {
      minCredit: 0,
      maxAccounts: process.env.MAX_ACCOUNTS,
      user: user,
      customerOtpSetting: customerOtpSetting,
      isSpecialSite: req.session.isSpecialSite,
    },
  };
}, sessionOptions);

export default NewCustomer;
