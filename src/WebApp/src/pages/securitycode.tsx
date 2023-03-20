import SecurityCodeForm from "@components/SecurityCodeForm/Index";
import { useState } from "react";

export default function SecurityCode() {
  const [isSecurityCodeVerified, setIsSecurityCodeVerified] = useState(false);
  const onVerifySecurityCodeSuccess = () => {
    setIsSecurityCodeVerified(true);
  };

  return <SecurityCodeForm username="Master1" onVerifySuccess={onVerifySecurityCodeSuccess} />;
}
