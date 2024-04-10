import Layout from "@components/Layout";
import React, { useState } from "react";
import useTranslation from "next-translate/useTranslation";
import { sessionOptions, User } from "src/lib/session";
import { withIronSessionSsr } from "iron-session/next";
import { fetchWrapper } from "src/lib/fetch-client";
import VerifyOtp from "@components/OtpForm";
import Message from "@components/Message";
import SecurityCodeForm from "@components/SecurityCodeForm";
import { EditAutoTransferPage } from "@features/EditAutoTransferPage";

type CustomerOtpSetting = {
  isEnableSophosOtp: boolean;
  isEnableTelegramOtp: boolean;
};

type EditAutoTransferSettingModel = {
  user: User;
  isSpecialSite: boolean;
  isRequireSecurityCode: boolean;
  customerOtpSetting: CustomerOtpSetting;
  adbtStatus: boolean;
};


const EditAutoTransferSetting = ({ user, isRequireSecurityCode, isSpecialSite, customerOtpSetting, adbtStatus, ...props }: EditAutoTransferSettingModel) => {
  const { t } = useTranslation("editautotransfer");
  const [isSecurityCodeOrOTPVerified, setIsSecurityCodeOrOTPVerified] = useState(user.isVerifyOtpOrSC);

  const onVerifySecurityCodeOrOTPSuccess = () => {
    setIsSecurityCodeOrOTPVerified(true);
  };

  function render() {
    if (!isSecurityCodeOrOTPVerified) {
      if (customerOtpSetting.isEnableSophosOtp || customerOtpSetting.isEnableTelegramOtp) {
        return <VerifyOtp {...customerOtpSetting} username={user.username} onVerifySuccess={onVerifySecurityCodeOrOTPSuccess} />;
      }

      if (isRequireSecurityCode) {
        return <SecurityCodeForm username={user.username} salt={user.salt} onVerifySuccess={onVerifySecurityCodeOrOTPSuccess} />;
      }
    }

    if(adbtStatus) {
      return <Message message={t("adbtChangeError")} isError></Message>;    
    }

    return (<EditAutoTransferPage user={user} {...props}></EditAutoTransferPage>);
  }; 

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
  const adbtStatus = await fetchWrapper.get<boolean>(`${process.env.TELEGRAM_API_URL}/AutoTransfer/IsAllDayBalanceTransfer?custId=${user?.custId}`);
   
  return {
    props: {
      user: user,
      isRequireSecurityCode: securityCode !== "" && securityCode !== undefined && securityCode !== null,
      isSpecialSite: req.session.isSpecialSite,
      customerOtpSetting: customerOtpSetting,
      adbtStatus: adbtStatus
    },
  };
}, sessionOptions);

export default EditAutoTransferSetting;