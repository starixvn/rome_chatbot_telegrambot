import { withIronSessionSsr } from "iron-session/next";
import useTranslation from "next-translate/useTranslation";
import React, { useState } from "react";
import { fetchWrapper } from "src/lib/fetch-client";
import { sessionOptions, User } from "src/lib/session";

import Layout from "@components/Layout";
import Message from "@components/Message";
import VerifyOtp from "@components/OtpForm";
import SecurityCodeForm from "@components/SecurityCodeForm";
import { CopyCustomer } from "@features/CopyCustomer";

type CustomerOtpSetting = {
  isEnableSophosOtp: boolean;
  isEnableTelegramOtp: boolean;
};

type Result = {
  code: number;
  message: string;
  isSuccessful: boolean;
};

type Props = {
  minCredit: number;
  maxAccounts: number;
  user: User;
  isRequireSecurityCode: boolean;
  customerOtpSetting: CustomerOtpSetting;
  isSpecialSite: boolean;
  limitedDownlineResult: Result;
};

const NewCustomer = ({ user, isRequireSecurityCode, customerOtpSetting, isSpecialSite, limitedDownlineResult, ...props }: Props) => {
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

    if (!limitedDownlineResult.isSuccessful) {
      return <Message message={limitedDownlineResult.message} isError />;
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
  const { user } = req.session;
  const securityCode = await fetchWrapper.get<string>(`${process.env.CUSTOMER_API}/security-code?custId=${user?.custId}&subAccId=${user?.subAccId}`);
  const customerOtpSetting = await fetchWrapper.get<CustomerOtpSetting>(`${process.env.CUSTOMER_API}/customer/otp/${user?.custId}/${user?.subAccId}`);
  const limitedDownlineResult = await fetchWrapper.post<Result>(`${process.env.CUSTOMER_API}/customer/CheckAllLimitedRules`, {
    custId: user?.custId,
    custLevel: user?.customerLevel,
    actorUsername: user?.username,
    language: user?.language,
  });

  return {
    props: {
      minCredit: 0,
      maxAccounts: process.env.MAX_ACCOUNTS,
      user: user,
      isRequireSecurityCode: securityCode !== "" && securityCode !== undefined && securityCode !== null,
      customerOtpSetting: customerOtpSetting,
      isSpecialSite: req.session.isSpecialSite,
      limitedDownlineResult,
    },
  };
}, sessionOptions);

export default NewCustomer;
