/* eslint-disable react-hooks/exhaustive-deps */
import React, { useEffect, useState } from "react";
import { CheckMarkIcon, ErrorIcon } from "@components/Icons";
import { components, GroupBase, OptionProps, SingleValue, StylesConfig } from "react-select";
import AsyncCreatableSelect from "react-select/async-creatable";
import Select from "react-select/dist/declarations/src/Select";
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

const checkIsDownLineUser = async (username: string) => {
  const url = `api/customer/downline/${username}`;
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
        <>
          <div>
            {props.data.userName} {props.data.userName2 && `(${props.data.userName2})`}
          </div>
          <div>
            {props.data.firstName} {props.data.lastName}
          </div>
        </>
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

  const handleOnChange = async (option: SingleValue<CustomerInfoOption>) => {
    if (option) {
      const data = await checkIsDownLineUser(option.value);
      if (data.isDownLineUser) {
        onChange(option.value);
        setIsCorrectName(ErrorMode.Correct);
        const data = await getCustomerMaxCredit(option.value);
        changeMaxCredit(data.maxCredit);
      } else {
        onChange("");
        setIsCorrectName(ErrorMode.InCorrect);
      }
    }
  };

  const clearSingleValue = () => {
    selectInputRef?.clearValue();
  };

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
      <div className={styles.icon}>
        {isCorrectName === ErrorMode.Correct && <CheckMarkIcon height={16} width={16} />}
        {isCorrectName === ErrorMode.InCorrect && <ErrorIcon height={16} width={16} />}
      </div>
    </div>
  );
};

const customerStyles: StylesConfig<CustomerInfoOption> = {
  control: (baseStyles) => ({
    ...baseStyles,
    borderRadius: "0px",
    minHeight: "30px",
    height: "30px",
    width: "100%",
    display: "inline-block",
    paddingLeft: "3px",
  }),

  container: (provided) => ({
    ...provided,
    display: "inline-block",
    width: "100%",
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
