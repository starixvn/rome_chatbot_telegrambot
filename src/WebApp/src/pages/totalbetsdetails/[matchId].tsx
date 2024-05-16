import { withIronSessionSsr } from "iron-session/next";
import useTranslation from "next-translate/useTranslation";
import React, { useState } from "react";
import { fetchWrapper } from "src/lib/fetch-client";
import { sessionOptions, User } from "src/lib/session";

import Layout from "@components/Layout";
import VerifyOtp from "@components/OtpForm";
import { TotalBetsDetails } from "@features/TotalBetsDetails";
import { SoccerIcon } from "@components/Icons";
import { useRouter } from "next/router";
import BlockUI from "@components/BlockUI";

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
  matchId: number;
  origTotalBetsDetailsData: any;
};

type TotalBetsDetailsViewModel = {
  leagues: any;
  betTypes: any;
  toBetTypes: any;
  collapseBetTypeStatus: any;
  setCollapseBetTypeStatus: any;
};

const TotalBetsDetailsPage = ({ 
  user, isRequireSecurityCode, customerOtpSetting, isSpecialSite, matchId, origTotalBetsDetailsData, ...props }: Props) => {
  const router = useRouter();
  const { t } = useTranslation("totalbets");
  const [isSecurityCodeOrOTPVerified, setIsSecurityCodeOrOTPVerified] = useState(user.isVerifyOtpOrSC);
  const [totalBetsDetailsData, setTotalBetsDetailsData] = useState<TotalBetsDetailsViewModel | null>(origTotalBetsDetailsData);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [collapseBetTypeStatus, setCollapseBetTypeStatus] = useState({});

  const onVerifySecurityCodeOrOTPSuccess = () => {
    setIsSecurityCodeOrOTPVerified(true);
  };

  function render() {
    if (!isSecurityCodeOrOTPVerified) {
      if (customerOtpSetting.isEnableSophosOtp || customerOtpSetting.isEnableTelegramOtp) {
        return <VerifyOtp {...customerOtpSetting} username={user.username} onVerifySuccess={onVerifySecurityCodeOrOTPSuccess} />;
      }
    }

    return <TotalBetsDetails user={user} 
      totalBetsDetailsData={totalBetsDetailsData} 
      collapseBetTypeStatus={collapseBetTypeStatus}
      setCollapseBetTypeStatus={setCollapseBetTypeStatus}
      isSpecialSite={isSpecialSite}
      {...props} />;
  }

  const getTotalBetsDetails = async (callback: any) => {
    setIsSubmitting(true);
    const url = `/api/totalbetsdetails/details`;
    const body = {
      custId: user?.custId,
      language: user?.language,
      custLevel: user?.customerLevel,
      isApplyPT: !isSpecialSite,
      matchId: matchId
    };

    const data = await fetchWrapper.post<any>(url, body, router);
    setCollapseBetTypeStatus({});
    setTotalBetsDetailsData(data);
    callback && callback();
    setIsSubmitting(false);
  } 

  return (
    <Layout title={t("headerTotalBetsDetails")} roleId={user.customerLevel} 
      isSpecialSite={isSpecialSite}
      showAutoRefresh={true}
      titleIcon={<SoccerIcon height={20} width={20} />}
      refreshAction={getTotalBetsDetails}>
      {isSubmitting && <BlockUI />}
      {!isSubmitting && render()}
    </Layout>
  );
};

export const getServerSideProps = withIronSessionSsr(async function ({ req, res, query }) {
  const { user } = req.session;
  const securityCode = await fetchWrapper.get<string>(`${process.env.CUSTOMER_API}/security-code?custId=${user?.custId}&subAccId=${user?.subAccId}`);
  const customerOtpSetting = await fetchWrapper.get<CustomerOtpSetting>(`${process.env.CUSTOMER_API}/customer/otp/${user?.custId}/${user?.subAccId}`);
  
  const matchId = query?.matchId;

  const detailsParam = { 
    custId: user?.custId,
    isApplyPT: !req.session.isSpecialSite,
    matchId: matchId,
    subAccId: user?.subAccId,
    customerLevel: user?.customerLevel,
    language: user?.language,
  };
  
  const totalBetsDetailsData = await fetchWrapper.post<any>(`${process.env.TELEGRAM_API_URL}/totalbets/details`, detailsParam);
 
  return {
    props: {
      minCredit: 0,
      maxAccounts: process.env.MAX_ACCOUNTS,
      user: user,
      isRequireSecurityCode: securityCode !== "" && securityCode !== undefined && securityCode !== null,
      customerOtpSetting: customerOtpSetting,
      isSpecialSite: req.session.isSpecialSite,
      matchId: matchId,
      origTotalBetsDetailsData: totalBetsDetailsData
    },
  };
}, sessionOptions);

export default TotalBetsDetailsPage;
