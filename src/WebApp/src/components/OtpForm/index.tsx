import BlockUI from "@components/BlockUI";
import { Countdown } from "@components/Countdown";
import useTranslation from "next-translate/useTranslation";
import React, { useState } from "react";
import { useForm } from "react-hook-form";
import { fetchWrapper } from "src/lib/fetch-wrapper";
import styles from "./Otp.module.scss";

type Props = {
  actorName: string;
  isEnableSophosOtp: boolean;
  isEnableTelegramOtp: boolean;
  onVerifySuccess: () => void;
};

const VerifyOtp = (props: Props) => {
  const { t } = useTranslation("common");
  const methods = useForm({ mode: "onSubmit", shouldFocusError: false });
  const { handleSubmit, register, formState } = methods;

  const [countdown, setCountdown] = useState(false);

  const onSubmit = handleSubmit(async (formData) => {
    const result = await fetchWrapper.post("/api/otp/verify", { otp: formData.otp, isEnableTelegramOtp: props.isEnableTelegramOtp });
    if (result.isSuccessful) {
      props.onVerifySuccess();
    }
  });

  const handleGenerateOTP = async () => {
    setCountdown(true);
    await fetchWrapper.post("/api/otp/telegram/generate");
  };

  return (
    <div className={styles.otp}>
      {formState.isSubmitting && <BlockUI />}
      <form onSubmit={onSubmit}>
        <p>
          {t("Hi")} <span>{props.actorName}</span>
        </p>
        <p>{t("EnterOtp")}</p>

        <label>{t("Otp")}</label>
        <input {...register("otp", { required: true })} type="number" />
        <div className={styles.buttons}>
          {props.isEnableTelegramOtp && (
            <button className="btn-submit" type="button" onClick={handleGenerateOTP} disabled={countdown}>
              {`${t("GenerateOTP")} `}
              {countdown && <Countdown time={180} onFinishCount={() => setCountdown(false)} />}
            </button>
          )}
          <button className="btn-submit" type="submit">
            Submit
          </button>
        </div>
      </form>
    </div>
  );
};

export default VerifyOtp;
