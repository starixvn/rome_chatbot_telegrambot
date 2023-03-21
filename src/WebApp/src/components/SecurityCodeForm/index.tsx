import BlockUI from "@components/BlockUI";
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
  const [error, setError] = useState("");
  const [blockUI, setBlockUI] = useState(false);

  const handleShowSecurityCode = () => {
    setSecurityCodeShown(!securityCodeShown);
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setBlockUI(true);

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
    setBlockUI(false);
  };

  return (
    <>
      <form className={styles["security-code"]} onSubmit={handleSubmit}>
        <label className={styles["form-lable"]} htmlFor="SecurityCode">
          {t("SecurityCode")}
        </label>
        {error && <Message message={error} />}
        <p className={styles["form-name"]}>
          {t("Hi")} {username},
        </p>
        <p className={styles["form-note"]}>{t("SCNote")}</p>
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
          <label htmlFor="checkbox" className={styles["has-checkbox"]}>
            <input type="checkbox" id="checkbox" checked={securityCodeShown} onChange={handleShowSecurityCode} />
            {t("ShowSecurityCode")}
          </label>
        </div>
      </form>
      {blockUI && <BlockUI />}
    </>
  );
};

export default SecurityCodeForm;
