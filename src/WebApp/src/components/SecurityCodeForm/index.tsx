import React, { useState } from "react";
import { useForm } from "react-hook-form";
import useTranslation from "next-translate/useTranslation";
import { fetchWrapper } from "src/lib/fetch-wrapper";
import BlockUI from "@components/BlockUI";
import Message from "@components/Message";
import { EyeCloseIcon, EyeIcon } from "@components/Icons";
import styles from "./SecurityCode.module.scss";

type Props = {
  username: string;
  onVerifySuccess: () => void;
};

const SecurityCodeForm = ({ username, onVerifySuccess }: Props) => {
  const { t } = useTranslation("common");
  const { handleSubmit, register, formState } = useForm({ mode: "onSubmit", shouldFocusError: false });

  const [securityCodeShown, setSecurityCodeShown] = useState(false);
  const [error, setError] = useState("");

  const handleShowSecurityCode = () => {
    setSecurityCodeShown(!securityCodeShown);
  };

  const onSubmit = handleSubmit(async (formData) => {
    const result = await fetchWrapper.post("/api/security-code/verify", formData);
    if (result.isSuccessful) {
      onVerifySuccess();
    } else {
      setError(t("WrongSCMessage"));
    }
  });

  return (
    <div className={styles.sc}>
      {formState.isSubmitting && <BlockUI />}
      <form onSubmit={onSubmit}>
        {error && (
          <div className={styles.mess}>
            <Message message={error} isError />
          </div>
        )}
        <p className={styles.title}>{t("SecurityCode")}</p>
        <p>
          {t("Hi")} <span>{username}</span>
        </p>
        <p>{t("SCNote")}</p>
        <div className={styles.row}>
          <input
            {...register("securityCode", { required: true })}
            pattern="[0-9]*"
            autoComplete="off"
            maxLength={6}
            type={securityCodeShown ? "text" : "password"}
          />
          <div className={styles.eye} onClick={handleShowSecurityCode}>
            {securityCodeShown ? <EyeIcon height={16} width={16} /> : <EyeCloseIcon height={16} width={16} />}
          </div>
        </div>
        <button className="btn-submit" type="submit">
          {t("Verify")}
        </button>
      </form>
    </div>
  );
};

export default SecurityCodeForm;
