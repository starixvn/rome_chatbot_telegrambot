import { withIronSessionSsr } from "iron-session/next";
import useTranslation from "next-translate/useTranslation";
import React, { useState } from "react";
import { fetchWrapper } from "src/lib/fetch-client";
import { sessionOptions, User } from "src/lib/session";

import Layout from "@components/Layout";
import VerifyOtp from "@components/OtpForm";
import { TotalBets } from "@features/TotalBets";
import { SoccerIcon } from "@components/Icons";
import { useRouter } from "next/router";
import BlockUI from "@components/BlockUI";
import {  getCustomerStatus } from "../lib/with-auth";

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
  origTotalBetsData: any;
};

type TotalBetsViewModel = {
  matches: any;
};


const TotalBetsPage = ({ 
  user, isRequireSecurityCode, 
  customerOtpSetting, isSpecialSite,
  origTotalBetsData, ...props }: Props) => {
  const router = useRouter();
  const { t } = useTranslation("totalbets");
  const [isSecurityCodeOrOTPVerified, setIsSecurityCodeOrOTPVerified] = useState(user?.isVerifyOtpOrSC);
  const [totalBetsData, setTotalBetsData] = useState<TotalBetsViewModel | null>(origTotalBetsData);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const onVerifySecurityCodeOrOTPSuccess = () => {
    setIsSecurityCodeOrOTPVerified(true);
  };

  const getTotalBets = async (callback: any) => {
    setIsSubmitting(true);

    const url = `/api/totalbets/summary`;
    const body = { };
    const data = await fetchWrapper.post<any>(url, body, router);
    setTotalBetsData(data);
    callback && callback();
    setIsSubmitting(false);
 };

  function render() {
    if (!isSecurityCodeOrOTPVerified) {
      if (customerOtpSetting.isEnableSophosOtp || customerOtpSetting.isEnableTelegramOtp) {
        return <VerifyOtp {...customerOtpSetting} username={user.username} onVerifySuccess={onVerifySecurityCodeOrOTPSuccess} />;
      }
    }

    return <TotalBets user={user} totalBetsData={totalBetsData} {...props} />;
  }

  return (
    <Layout title={t("headerTotalBets")} roleId={user.customerLevel} 
      isSpecialSite={isSpecialSite}
      showAutoRefresh={true}
      titleIcon={<SoccerIcon height={20} width={20} />}
      refreshAction={getTotalBets}>
      {isSubmitting && <BlockUI />}
      {!isSubmitting && render()}
    </Layout>
  );
};

export const getServerSideProps = withIronSessionSsr(async function ({ req, res }) {
  const { user } = req.session;
  const securityCode = await fetchWrapper.get<string>(`${process.env.CUSTOMER_API}/security-code?custId=${user?.custId}&subAccId=${user?.subAccId}`);
  const customerOtpSetting = await fetchWrapper.get<CustomerOtpSetting>(`${process.env.CUSTOMER_API}/customer/otp/${user?.custId}/${user?.subAccId}`);
  
  const params = { 
    custId: user?.custId,
    isApplyPT: !req.session.isSpecialSite,
    subAccId: user?.subAccId,
    customerLevel: user?.customerLevel,
    language: user?.language
  };
  
    const totalBetsData = await fetchWrapper.post<any>(`${process.env.TELEGRAM_API_URL}/totalbets/summary`, params);

  return {
    props: {
      minCredit: 0,
      maxAccounts: process.env.MAX_ACCOUNTS,
      user: user,
      isRequireSecurityCode: securityCode !== "" && securityCode !== undefined && securityCode !== null,
      customerOtpSetting: customerOtpSetting,
      isSpecialSite: req.session.isSpecialSite,
      origTotalBetsData: totalBetsData
    },
  };
}, sessionOptions);

export default TotalBetsPage;
