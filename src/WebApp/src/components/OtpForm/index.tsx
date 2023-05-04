import React, { useState } from "react";
import { useForm } from "react-hook-form";
import useTranslation from "next-translate/useTranslation";
import cn from "classnames";
import { fetchWrapper } from "src/lib/fetch-client";
import Message from "@components/Message";
import BlockUI from "@components/BlockUI";
import { Countdown } from "@components/Countdown";
import styles from "./Otp.module.scss";
import { useRouter } from "next/router";

type Props = {
  username: string;
  isEnableSophosOtp: boolean;
  isEnableTelegramOtp: boolean;
  onVerifySuccess: () => void;
};

const VerifyOtp = (props: Props) => {
  const router = useRouter();
  const { t } = useTranslation("common");
  const { handleSubmit, register, formState } = useForm({ mode: "onSubmit", shouldFocusError: false });

  const [countdown, setCountdown] = useState(false);
  const [error, setError] = useState("");

  const onSubmit = handleSubmit(async (formData) => {
    if (formData.otp === "") {
      setError(t("RequiredMessage"));
      return;
    }
    const result = await fetchWrapper.post<any>("/api/otp/verify", { otp: formData.otp, isEnableTelegramOtp: props.isEnableTelegramOtp }, router);
    if (result.isSuccessful) {
      props.onVerifySuccess();
    } else {
      setError(t("WrongOTPMessage"));
    }
  });

  const handleGenerateOTP = async () => {
    setCountdown(true);
    const result = await fetchWrapper.post<any>("/api/otp/telegram/generate", router);
    if (!result.isSuccessful) {
      setError(t("OTPViaTelegramDisabled"));
    }
  };

  return (
    <div className={styles.otp}>
      {formState.isSubmitting && <BlockUI />}
      <form onSubmit={onSubmit}>
        {error && (
          <div className={styles.mess}>
            <Message message={error} isError />
          </div>
        )}
        <p className={styles.title}>{t("OTPValidation")}</p>
        <p>
          {t("Hi")} <span>{props.username}</span>
        </p>
        <p>{t("OtpNote")}</p>
        <input {...register("otp")} pattern="[0-9]*" autoComplete="off" maxLength={6} />
        <div className={styles.buttons}>
          {props.isEnableTelegramOtp && (
            <button className={cn("btn-submit", styles["btn-genOtp"])} type="button" onClick={handleGenerateOTP} disabled={countdown}>
              {`${t("GenerateOTP")} `}
              {countdown && <Countdown time={180} onFinishCount={() => setCountdown(false)} />}
            </button>
          )}
          <button className="btn-submit" type="submit">
            {t("Verify")}
          </button>
        </div>
      </form>
    </div>
  );
};

export default VerifyOtp;
