import { Countdown } from "@components/Countdown";
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
  const methods = useForm({ mode: "onSubmit", shouldFocusError: false });
  const { handleSubmit, register } = methods;

  const [countdown, setCountdown] = useState(false);

  const onSubmit = handleSubmit(async (formData) => {
    const result = await fetchWrapper.post("/api/otp/verify", { otp: formData.otp, isEnableTelegramOtp: props.isEnableTelegramOtp });
    if (result.isSuccessful) {
      props.onVerifySuccess();
    }
  });

  const handleGenerateOTP = async () => {
    await fetchWrapper.post("/api/otp/telegram/generate");
    setCountdown(true);
  };

  return (
    <form onSubmit={onSubmit}>
      <div>Hi {props.actorName}</div>
      <div>Please enter your OTP before proceeding!</div>

      <label>OTP</label>
      <input {...register("otp")} />
      {props.isEnableTelegramOtp && (
        <button type="button" onClick={handleGenerateOTP} disabled={countdown}>
          Generate OTP
          {countdown && <Countdown time={180} onFinishCount={() => setCountdown(false)} />}
        </button>
      )}
      <button type="submit">Submit</button>
    </form>
  );
};

export default VerifyOtp;
