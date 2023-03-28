/* eslint-disable react-hooks/exhaustive-deps */
import { CheckMarkIcon, ErrorIcon, EyeCloseIcon, EyeIcon } from "@components/Icons";
import React, { forwardRef, useEffect, useRef, useState } from "react";
import { NumericFormat } from "react-number-format";
import { getNumberOfCommaString } from "src/lib/number";
import styles from "./Inputs.module.scss";

type InputFieldProps = {
  onChange?: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onBlur?: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onKeyUp?: () => void;
};

type NumberInputProps = InputFieldProps & {
  min?: number;
  max?: number;
};

type PasswordFieldProps = InputFieldProps & {
  validate?: (password: string) => boolean;
};

export const NumberInputField = forwardRef<HTMLInputElement, NumberInputProps>(function NumberInputField({ min = 0, max, ...props }, ref) {
  const [isValidValue, setIsValidValue] = useState<boolean | undefined>(undefined);
  const value = useRef(0);

  useEffect(() => {
    checkValidValue(value.current);
  }, [min, max]);

  const handleOnChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const inputValue = getNumberOfCommaString(e.target.value);
    e.target.value = inputValue.toString();
    props.onChange && props.onChange(e);
    value.current = inputValue;
    checkValidValue(inputValue);
  };

  const checkValidValue = (value: number) => {
    if ((typeof min === "number" && value < min) || (typeof max === "number" && value > max)) {
      setIsValidValue(false);
    } else {
      setIsValidValue(true);
    }
  };

  return (
    <div className={styles.numberField}>
      <NumericFormat {...props} onChange={handleOnChange} defaultValue={0} thousandSeparator="," decimalScale={2} />
      <div className={styles.icon}>
        {typeof min === "number" &&
          typeof max === "number" &&
          isValidValue !== undefined &&
          (isValidValue ? <CheckMarkIcon height={16} width={16} /> : <ErrorIcon height={16} width={16} />)}
      </div>
    </div>
  );
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
    <div className={styles.password}>
      <input type={displayPassword ? "text" : "password"} {...props} onChange={handleOnChange} onKeyUp={handleOnKeyUp} />
      <div className={styles.eye} onClick={handleDisplayPassword}>
        {displayPassword ? <EyeIcon /> : <EyeCloseIcon />}
      </div>
      <div className={styles.icon}>
        {isValidPassword !== undefined && (isValidPassword ? <CheckMarkIcon height={16} width={16} /> : <ErrorIcon height={16} width={16} />)}
      </div>
    </div>
  );
});
