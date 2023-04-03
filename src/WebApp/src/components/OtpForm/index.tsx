import BlockUI from "@components/BlockUI";
import { Countdown } from "@components/Countdown";
import useTranslation from "next-translate/useTranslation";
import React, { useState } from "react";
import { useForm } from "react-hook-form";
import { fetchWrapper } from "src/lib/fetch-wrapper";

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
    <>
      {formState.isSubmitting && <BlockUI />}
      <form onSubmit={onSubmit}>
        <div>
          {t("Hi")} {props.actorName}
        </div>
        <div>{t("EnterOtp")}</div>

        <label>{t("Otp")}</label>
        <input {...register("otp")} />
        {props.isEnableTelegramOtp && (
          <button type="button" onClick={handleGenerateOTP} disabled={countdown}>
            Generate OTP
            {countdown && <Countdown time={180} onFinishCount={() => setCountdown(false)} />}
          </button>
        )}
        <button type="submit">Submit</button>
      </form>
    </>
  );
};

export default VerifyOtp;
