import { CheckMarkIcon, ErrorIcon, EyeCloseIcon, EyeIcon } from "@components/Icons";
import React, { forwardRef, useRef, useState } from "react";
import { NumericFormat } from "react-number-format";

type InputFieldProps = {
  onChange?: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onBlur?: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onKeyUp?: () => void;
};

type PasswordFieldProps = InputFieldProps & {
  validate?: (password: string) => boolean;
};

export const NumberInputField = forwardRef<HTMLInputElement, InputFieldProps>(function NumberInputField(props, ref) {
  return <NumericFormat {...props} thousandSeparator="," decimalScale={2} />;
});

export const PasswordField = forwardRef<HTMLInputElement, PasswordFieldProps>(function PasswordField({ validate, ...props }, ref) {
  const [displayPassword, setDisplayPassword] = useState(false);
  const [isValidPassword, setIsValidPassword] = useState<boolean | undefined>(undefined);
  const password = useRef("");

  const handleDisplayPassword = () => {
    setDisplayPassword(!displayPassword);
  };

  const handleOnChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    props.onChange && props.onChange(e);
    password.current = e.target.value;
  };

  const handleOnKeyUp = () => {
    const isValid = validate && validate(password.current);
    isValid ? setIsValidPassword(true) : setIsValidPassword(false);
  };

  return (
    <>
      <input type={displayPassword ? "text" : "password"} {...props} onChange={handleOnChange} onKeyUp={handleOnKeyUp} />
      <span onClick={handleDisplayPassword}>{displayPassword ? <EyeIcon /> : <EyeCloseIcon />}</span>
      {isValidPassword !== undefined && (isValidPassword ? <CheckMarkIcon height={16} width={16} /> : <ErrorIcon height={16} width={16} />)}
    </>
  );
});
