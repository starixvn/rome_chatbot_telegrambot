import Message from "@components/Message";
import useTranslation from "next-translate/useTranslation";
import React, { FormEvent, useState } from "react";
import styles from "./SecurityCode.module.scss";

type Props = {
  username: string;
  onVerifySuccess: () => void;
};

const SecurityCodeForm = ({ username, onVerifySuccess }: Props) => {
  const { t } = useTranslation("common");
  const [securityCodeShown, setSecurityCodeShown] = useState(false);
  const [securityCode, setSecurityCode] = useState("");
  const [error, setError] = useState("Wrong Security Code. Please try again.");

  const handleShowSecurityCode = () => {
    setSecurityCodeShown(!securityCodeShown);
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    const formData = new URLSearchParams();
    formData.append("securityCode", securityCode);
    const response = await fetch("/api/security-code/verify", {
      method: "POST",
      body: formData.toString(),
      headers: {
        "content-type": "application/x-www-form-urlencoded",
      },
    });

    const result = await response.json();
    if (result.isSuccessful) {
      onVerifySuccess();
    } else {
      setError(result.message);
    }
  };

  return (
    <form className={styles["security-code"]} onSubmit={handleSubmit}>
      <label htmlFor="SecurityCode">{t("SecurityCode")}</label>
      {error && <Message message={error} />}
      <p>
        {t("Hi")} {username},
      </p>
      <p>{t("SCNote")}</p>
      <div className={styles["form-group"]}>
        <input
          className={styles["form-control"]}
          id="SecurityCode"
          name="SecurityCode"
          type={securityCodeShown ? "text" : "password"}
          pattern="[0-9]*"
          autoComplete="off"
          maxLength={6}
          value={securityCode}
          onChange={(e) => setSecurityCode(e.target.value)}
        />
        <input type="submit" value={t("Verify")} />
      </div>
      <div className={styles["form-group"]}>
        <input type="checkbox" id="checkbox" checked={securityCodeShown} onChange={handleShowSecurityCode} />
        <label htmlFor="checkbox">{t("ShowSecurityCode")}</label>
      </div>
    </form>
  );
};

export default SecurityCodeForm;
