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
  isRequireSecurityCode: boolean;
  customerOtpSetting: CustomerOtpSetting;
  isSpecialSite: boolean;
};

const NewCustomer = ({ user, isRequireSecurityCode, customerOtpSetting, isSpecialSite, ...props }: Props) => {
  const { t } = useTranslation("newcustomer");
  const [isSecurityCodeOrOTPVerified, setIsSecurityCodeOrOTPVerified] = useState(user.isVerifyOtpOrSC);
  const onVerifySecurityCodeOrOTPSuccess = () => {
    setIsSecurityCodeOrOTPVerified(true);
  };

  function render() {
    if (isSecurityCodeOrOTPVerified) {
      return <CopyCustomer user={user} {...props} />;
    }

    if (customerOtpSetting.isEnableSophosOtp || customerOtpSetting.isEnableTelegramOtp) {
      return <VerifyOtp {...customerOtpSetting} username={user.username} onVerifySuccess={onVerifySecurityCodeOrOTPSuccess} />;
    }

    if (isRequireSecurityCode) {
      return <SecurityCodeForm username={user.username} salt={user.salt} onVerifySuccess={onVerifySecurityCodeOrOTPSuccess} />;
    }

    return <CopyCustomer user={user} {...props} />;
  }

  return (
    <Layout title={t("header")} roleId={user.customerLevel} isSpecialSite={isSpecialSite}>
      {render()}
    </Layout>
  );
};

export const getServerSideProps = withIronSessionSsr(async function ({ req, res }) {
  const user = req.session.user;

  Sentry.captureMessage(`${new Date().getMinutes()}:${new Date().getMilliseconds()}: custId: ${user?.custId} -> salt: ${user?.salt} -> hash: ${user?.hash}`);

  const securityCode = await fetchWrapper.get<string>(`${process.env.CUSTOMER_API}/security-code?custId=${user?.custId}&subAccId=${user?.subAccId}`);
  const customerOtpSetting = await fetchWrapper.get<CustomerOtpSetting>(`${process.env.CUSTOMER_API}/customer/otp/${user?.custId}/${user?.subAccId}`);
  return {
    props: {
      minCredit: 0,
      maxAccounts: process.env.MAX_ACCOUNTS,
      user: user,
      isRequireSecurityCode: securityCode !== "" && securityCode !== undefined && securityCode !== null,
      customerOtpSetting: customerOtpSetting,
      isSpecialSite: req.session.isSpecialSite,
    },
  };
}, sessionOptions);

export default NewCustomer;
