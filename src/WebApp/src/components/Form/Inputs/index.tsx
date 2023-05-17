/* eslint-disable react-hooks/exhaustive-deps */
import { CheckMarkIcon, ErrorIcon, EyeCloseIcon, EyeIcon } from "@components/Icons";
import React, { forwardRef, useEffect, useRef, useState } from "react";
import { NumericFormat } from "react-number-format";
import cn from "classnames";
import { getNumberOfCommaString } from "src/lib/number";
import styles from "./Inputs.module.scss";
import Message from "@components/Message";

type InputFieldProps = {
  onChange?: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onBlur?: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onKeyUp?: () => void;
};

type NumberInputProps = InputFieldProps & {
  min?: number;
  max?: number;
  messError?: string;
  placeholder?: string;
};

type PasswordFieldProps = InputFieldProps & {
  validate?: (password: string) => boolean;
  messError?: string;
};

export const NumberInputField = forwardRef<HTMLInputElement, NumberInputProps>(function NumberInputField({ min = 0, max, messError = "", ...props }, ref) {
  const [isValidValue, setIsValidValue] = useState<boolean | undefined>(undefined);
  const [showError, setShowError] = useState(false);
  const value = useRef(0);

  useEffect(() => {
    setIsValidValue(isValidInputValue(value.current));
  }, [min, max]);

  const handleOnChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const inputValue = getNumberOfCommaString(e.target.value);
    e.target.value = inputValue.toString();
    props.onChange && props.onChange(e);
    value.current = inputValue;
    setIsValidValue(isValidInputValue(inputValue));
    setShowError(!isValidInputValue(inputValue));
  };

  const handleOnBlur = (e: React.ChangeEvent<HTMLInputElement>) => {
    props.onBlur && props.onBlur(e);
    setShowError(false);
  };

  const handleOnFocus = () => {
    setShowError(!isValidInputValue(value.current));
  };

  const isValidInputValue = (value: number) => {
    return typeof min === "number" && value >= min && typeof max === "number" && value <= max;
  };

  return (
    <div className={styles.container}>
      <div className={cn(styles.numberField, { [styles.error]: isValidValue !== undefined && !isValidValue })}>
        <NumericFormat
          {...props}
          onChange={handleOnChange}
          onBlur={handleOnBlur}
          onFocus={handleOnFocus}
          defaultValue={0}
          thousandSeparator=","
          decimalScale={2}
          allowNegative={false}
          placeholder={props.placeholder}
          fixedDecimalScale={false}
        />
        <div className={styles.icon}>
          {typeof min === "number" &&
            typeof max === "number" &&
            isValidValue !== undefined &&
            (isValidValue ? <CheckMarkIcon height={16} width={16} /> : <ErrorIcon height={16} width={16} />)}
        </div>
      </div>
      {showError && messError && (
        <div className={styles.row}>
          <Message message={messError} isError></Message>
        </div>
      )}
    </div>
  );
});

export const PasswordField = forwardRef<HTMLInputElement, PasswordFieldProps>(function PasswordField({ validate, messError = "", ...props }, ref) {
  const [displayPassword, setDisplayPassword] = useState(false);
  const [showError, setShowError] = useState(false);
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
    setIsValidPassword(isValid);
    setShowError(!isValid);
  };

  const handleOnBlur = () => {
    setShowError(false);
  };

  const handleOnFocus = () => {
    if (password.current.length > 0) {
      const isValid = validate && validate(password.current);
      setShowError(!isValid);
    }
  };

  return (
    <div className={styles.container}>
      <div className={cn(styles.password, { [styles.error]: isValidPassword !== undefined && !isValidPassword })}>
        <input
          type={displayPassword ? "text" : "password"}
          {...props}
          onChange={handleOnChange}
          onKeyUp={handleOnKeyUp}
          onBlur={handleOnBlur}
          onFocus={handleOnFocus}
        />
        <div className={styles.eye} onClick={handleDisplayPassword}>
          {displayPassword ? <EyeIcon /> : <EyeCloseIcon />}
        </div>
        <div className={styles.icon}>
          {isValidPassword !== undefined && (isValidPassword ? <CheckMarkIcon height={16} width={16} /> : <ErrorIcon height={16} width={16} />)}
        </div>
      </div>
      {showError && messError && (
        <div className={styles.row}>
          <Message message={messError} isError></Message>
        </div>
      )}
    </div>
  );
});
