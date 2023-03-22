import Layout from "@components/Layout";
import SecurityCodeForm from "@components/SecurityCodeForm";
import { useState } from "react";

export default function SecurityCode() {
  const [isSecurityCodeVerified, setIsSecurityCodeVerified] = useState(false);

  const onVerifySecurityCodeSuccess = () => {
    setIsSecurityCodeVerified(true);
  };

  return (
    <Layout title="New Customer" roleId={4}>
      <SecurityCodeForm username="Master1" onVerifySuccess={onVerifySecurityCodeSuccess} />
    </Layout>
  );
}
