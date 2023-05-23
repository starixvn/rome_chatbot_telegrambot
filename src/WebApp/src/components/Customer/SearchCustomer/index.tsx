/* eslint-disable react-hooks/exhaustive-deps */
import React, { useEffect, useRef, useState } from "react";
import useTranslation from "next-translate/useTranslation";
import AsyncSelect from "react-select/async";
import { components, InputActionMeta, InputProps, OptionProps, SelectInstance, SingleValue, StylesConfig } from "react-select";
import { fetchWrapper } from "src/lib/fetch-client";
import { CheckMarkIcon, ErrorIcon } from "@components/Icons";
import Message from "@components/Message";
import styles from "./SearchCustomer.module.scss";
import { useRouter } from "next/router";

export type CustomerInfoOption = {
  userName: string;
  firstName: string;
  lastName: string;
  userName2: string;
  value: string;
};

type DownLineInfo = {
  isEnable: boolean;
  isDownLineUser: boolean;
};

type CustomerCredit = {
  credit: number;
  maxCredit: number;
};

enum ErrorMode {
  Init = 1,
  InCorrect,
  Correct,
}

const CustomOption = (props: OptionProps<CustomerInfoOption, false>) => {
  return (
    <components.Option {...props}>
      {props.data.userName ? (
        <div className={styles.item}>
          <div>
            {props.data.userName} {props.data.userName2 && `(${props.data.userName2})`}
          </div>
          <div>
            {props.data.firstName} {props.data.lastName}
          </div>
        </div>
      ) : (
        <span>{props.data.value}</span>
      )}
    </components.Option>
  );
};

const Input = (props: InputProps<CustomerInfoOption, false>) => (
  <components.Input
    {...props}
    isHidden={false}
    placeholder={props.selectProps.placeholder as string}
    style={{ minWidth: "300px", height: "28px", marginBottom: "2px", border: "none" }}
  />
);

type Props = {
  onChange: (existAccountName: string) => void;
  changeMaxCredit: (maxCredit: number) => void;
};

export const SearchCustomerExists = ({ onChange, changeMaxCredit }: Props) => {
  const router = useRouter();
  const { t } = useTranslation("newcustomer");
  const asyncRef = useRef<SelectInstance<CustomerInfoOption> | null>(null);
  const [isCorrectName, setIsCorrectName] = useState<ErrorMode>(ErrorMode.Init);
  const [value, setValue] = useState("");
  const [errorMessage, setErrorMessage] = useState("");

  useEffect(() => {
    setIsCorrectName(ErrorMode.Init);
    clearSingleValue();
  }, []);

  const handleOnChange = async (option: SingleValue<CustomerInfoOption>) => {
    if (option) {
      setValue(option.value);
      await handleCheckIsDownLine(option.value);
    }
  };

  const onInputChange = async (newValue: string, { action }: InputActionMeta) => {
    if (action === "input-change") {
      setValue(newValue);
      newValue && (await handleCheckIsDownLine(newValue));
    }
  };

  const handleCheckIsDownLine = async (username: string) => {
    const downLineInfo = await fetchWrapper.get<DownLineInfo>(`/api/customer/downline/${username}`, router);
    if (downLineInfo.isDownLineUser && downLineInfo.isEnable) {
      onChange(username);
      setIsCorrectName(ErrorMode.Correct);
      setErrorMessage("");
      const customerCredit = await fetchWrapper.get<CustomerCredit>(`/api/customer/credit/${username}`, router);
      changeMaxCredit(customerCredit.maxCredit);
    } else {
      onChange("");
      setIsCorrectName(ErrorMode.InCorrect);
      setErrorMessage(!downLineInfo.isEnable ? t("formError.accountDisabled") : t("formError.existAccount"));
    }
  };

  const customPromiseOptions = async (inputValue: string) => {
    const data = await fetchWrapper.get<any>(`/api/customer/search?query=${inputValue}`, router);
    return data.map((item: CustomerInfoOption) => {
      return {
        ...item,
        value: item.userName,
      };
    });
  };

  const clearSingleValue = () => {
    asyncRef.current?.clearValue();
  };

  return (
    <div className={styles.container}>
      <div className={styles.search}>
        <AsyncSelect
          inputValue={value}
          onInputChange={onInputChange}
          components={{ Option: CustomOption, Input: Input, SingleValue: () => null, Placeholder: () => null }}
          placeholder={t("sourceInputPlaceholder")}
          styles={{
            valueContainer: (provided) => ({
              ...provided,
              height: "32px",
              padding: 0,
              minWidth: "100%",
              border: isCorrectName === ErrorMode.InCorrect ? "1px solid red" : "",
            }),
            ...customerStyles,
          }}
          loadOptions={customPromiseOptions}
          onChange={handleOnChange}
          onBlur={() => setErrorMessage("")}
          noOptionsMessage={() => null}
          controlShouldRenderValue={false}
          formatOptionLabel={(option) => {
            return option.userName ? option.userName : option["value"];
          }}
        />
        <div className={styles.icon}>
          {isCorrectName === ErrorMode.Correct && <CheckMarkIcon height={16} width={16} />}
          {isCorrectName === ErrorMode.InCorrect && <ErrorIcon height={16} width={16} />}
        </div>
      </div>
      {errorMessage && (
        <div className={styles.row}>
          <Message message={errorMessage} isError />
        </div>
      )}
    </div>
  );
};

const customerStyles: StylesConfig<CustomerInfoOption> = {
  container: (provided) => ({
    ...provided,
    display: "inline-block",
    width: "100%",
    height: "32px",
    fontSize: "14px",
    fontWeight: "400",
    lineHeight: "22px",
  }),

  control: (baseStyles) => ({
    ...baseStyles,
    borderRadius: "0px",
    minHeight: "32px",
    height: "32px",
    width: "100%",
    display: "inline-block",
    border: "1px solid #d1d1d1",
    boxShadow: "none",
    "&:hover": {
      outline: "none",
    },
  }),

  menu: (provided) => ({
    ...provided,
    zIndex: 2,
    borderRadius: "none",
    marginTop: "1px",
  }),

  input: (provided) => ({
    ...provided,
    padding: 0,
    margin: 0,
  }),

  singleValue: (provided) => ({
    ...provided,
    padding: 0,
    paddingLeft: "5px",
    margin: 0,
  }),

  placeholder: (provided) => ({
    ...provided,
    padding: 0,
    paddingLeft: "5px",
    margin: 0,
  }),

  indicatorSeparator: () => ({
    display: "none",
  }),

  indicatorsContainer: (provided) => ({
    ...provided,
    display: "none",
  }),
};
