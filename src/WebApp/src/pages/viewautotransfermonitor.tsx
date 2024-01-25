import Layout from "@components/Layout";
import React, { useState } from "react";
import useTranslation from "next-translate/useTranslation";
import { sessionOptions, User } from "src/lib/session";
import VerifyOtp from "@components/OtpForm";
import { withIronSessionSsr } from "iron-session/next";
import { fetchWrapper } from "src/lib/fetch-client";
import Message from "@components/Message";
import { AutoTransferMonitorPage } from "@features/AutoTransferMonitorPage";

type CustomerOtpSetting = {
  isEnableSophosOtp: boolean;
  isEnableTelegramOtp: boolean;
};

type AutoTransferMonitoringModel = {
  user: User;
  isSpecialSite: boolean;
  customerOtpSetting: CustomerOtpSetting;
  adbtStatus: boolean;
};

const ViewAutoTransferMonitoring = ({ user, isSpecialSite, customerOtpSetting, adbtStatus, ...props }: AutoTransferMonitoringModel) => {
  const { t } = useTranslation("viewautotransfermonitor");
  const [isSecurityCodeOrOTPVerified, setIsSecurityCodeOrOTPVerified] = useState(user.isVerifyOtpOrSC);

  const onVerifySecurityCodeOrOTPSuccess = () => {
    setIsSecurityCodeOrOTPVerified(true);
  };

  function render() {
    if (!isSecurityCodeOrOTPVerified) {
      if (customerOtpSetting.isEnableSophosOtp || customerOtpSetting.isEnableTelegramOtp) {
        return <VerifyOtp {...customerOtpSetting} username={user.username} onVerifySuccess={onVerifySecurityCodeOrOTPSuccess} />;
      }
    }

    if(adbtStatus) {
      return <Message message={t("adbtChangeError")} isError></Message>;    
    }

    return <AutoTransferMonitorPage user={user} {...props} />;
  }; 

  return (
    <Layout title={t("header")} roleId={user.customerLevel} isSpecialSite={isSpecialSite}>
      {render()}
    </Layout>
  );
};

export const getServerSideProps = withIronSessionSsr(async function ({ req, res }) {
  const { user } = req.session;
  const customerOtpSetting = await fetchWrapper.get<CustomerOtpSetting>(`${process.env.CUSTOMER_API}/customer/otp/${user?.custId}/${user?.subAccId}`);
  const adbtStatus = await fetchWrapper.get<boolean>(`${process.env.TELEGRAM_API_URL}/AutoTransfer/IsAllDayBalanceTransfer?custId=${user?.custId}`);
   
  return {
    props: {
      user: user,
      isSpecialSite: req.session.isSpecialSite,
      customerOtpSetting: customerOtpSetting,
      adbtStatus: adbtStatus
    },
  };
}, sessionOptions);

export default ViewAutoTransferMonitoring;