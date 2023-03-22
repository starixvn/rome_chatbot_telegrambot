/* eslint-disable react-hooks/exhaustive-deps */
import React, { useEffect, useState } from "react";
import { CheckMarkIcon, ErrorIcon } from "@components/Icons";
import { components, GroupBase, OptionProps, SingleValue, StylesConfig } from "react-select";
import AsyncCreatableSelect from "react-select/async-creatable";
import Select from "react-select/dist/declarations/src/Select";
import { checkExistAccountName } from "../NewAccount";
import styles from "./SearchCustomer.module.scss";

export type CustomerInfoOption = {
  userName: string;
  firstName: string;
  lastName: string;
  userName2: string;
  value: string;
};

enum ErrorMode {
  Init = 1,
  InCorrect,
  Correct,
}

const getExistCustomers = async (query: string) => {
  const url = `/api/customer/search?query=${query}`;
  const res = await fetch(url);
  return await res.json();
};

const getCustomerMaxCredit = async (username: string) => {
  const url = `/api/customer/credit/${username}`;
  const result = await fetch(url);
  return await result.json();
};

export const customPromiseOptions = async (inputValue: string) => {
  const data = await getExistCustomers(inputValue);
  return data.map((item: CustomerInfoOption) => {
    return {
      ...item,
      value: item.userName,
    };
  });
};

const CustomOption = (props: OptionProps<CustomerInfoOption, false>) => {
  return (
    <components.Option {...props}>
      {props.data.userName ? (
        <span>
          {props.data.userName}: {props.data.userName2}
        </span>
      ) : (
        <span>{props.data.value}</span>
      )}
    </components.Option>
  );
};

type Props = {
  onChange: (existAccountName: string) => void;
  changeMaxCredit: (maxCredit: number) => void;
};

export const SearchCustomerExists = ({ onChange, changeMaxCredit }: Props) => {
  let selectInputRef: Select<CustomerInfoOption, boolean, GroupBase<CustomerInfoOption>> | null = null;
  const [isCorrectName, setIsCorrectName] = useState<ErrorMode>(ErrorMode.Init);

  useEffect(() => {
    setIsCorrectName(ErrorMode.Init);
    clearSingleValue();
  }, []);

  const checkExistUserName = async (username: string) => {
    const data = await checkExistAccountName(username);
    if (data.isExist) {
      setIsCorrectName(ErrorMode.Correct);
    } else {
      onChange("");
      setIsCorrectName(ErrorMode.InCorrect);
    }
  };

  const handleOnChange = async (option: SingleValue<CustomerInfoOption>) => {
    if (option) {
      const data = await getCustomerMaxCredit(option.value);
      onChange(option.value);
      await checkExistUserName(option.value);
      changeMaxCredit(data.maxCredit);
    }
  };

  const clearSingleValue = () => {
    selectInputRef?.clearValue();
  };

  console.log("render search");
  return (
    <div className={styles.search}>
      <AsyncCreatableSelect
        ref={(ref) => {
          selectInputRef = ref;
        }}
        components={{ Option: CustomOption }}
        placeholder=""
        styles={customerStyles}
        loadOptions={customPromiseOptions}
        onChange={handleOnChange}
        noOptionsMessage={() => null}
        formatOptionLabel={(option) => {
          return option.userName ? option.userName : option["value"];
        }}
      />
      {isCorrectName === ErrorMode.Correct && <CheckMarkIcon height={16} width={16} />}
      {isCorrectName === ErrorMode.InCorrect && <ErrorIcon height={16} width={16} />}
    </div>
  );
};

const customerStyles: StylesConfig<CustomerInfoOption> = {
  control: (baseStyles) => ({
    ...baseStyles,
    minHeight: "30px",
    height: "30px",
    width: "320px",
    display: "inline-block",
    paddingLeft: "3px",
  }),

  container: (provided) => ({
    ...provided,
    display: "inline-block",
  }),

  valueContainer: (provided) => ({
    ...provided,
    height: "30px",
    padding: "0 6px",
  }),

  input: (provided) => ({
    ...provided,
    margin: "0px 2px",
  }),

  indicatorSeparator: () => ({
    display: "none",
  }),

  indicatorsContainer: (provided) => ({
    ...provided,
    display: "none",
  }),
};
