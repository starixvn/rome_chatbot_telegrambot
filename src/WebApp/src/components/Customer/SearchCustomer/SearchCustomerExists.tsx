/* eslint-disable react-hooks/exhaustive-deps */
import React, { useEffect, useState } from "react";
import { CheckMarkIcon } from "@components/Icons/Icons";
import { components, GroupBase, OptionProps, SingleValue} from "react-select";
import AsyncCreatableSelect from "react-select/async-creatable";
import Select from "react-select/dist/declarations/src/Select";
import { checkExistAccountName } from "../NewAccount/NewAccountName";
import { customerStyles } from "./CustomStyle";

export type CustomerInfoOption = {
  userName: string,
  firstName: string,
  lastName: string,
  userName2: string
  value: string,
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

const CustomOption = (props:  OptionProps<CustomerInfoOption, false>) => {
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
  onChange: (existAccountName: string) => void,
  changeMaxCredit: (maxCredit: number) => void
}

export const SearchCustomerExists = ({ onChange, changeMaxCredit }: Props) => {
  let selectInputRef: Select<CustomerInfoOption, boolean, GroupBase<CustomerInfoOption>> | null = null;
  const [isCorrectName, setIsCorrectName] = useState(false);

  useEffect(() => {
    setIsCorrectName(false);
    clearSingleValue();
  }, []);

  const checkExistUserName = async (username: string) => {
    const data = await checkExistAccountName(username);
    if (data.isExist) {
      setIsCorrectName(true);
    } else {
      onChange("");
      setIsCorrectName(false);
    }
  };

  const handleOnChange = async (option: SingleValue<CustomerInfoOption>) => {
    if (option) {
      const data = await getCustomerMaxCredit(option.value);
      onChange(option.value);
      await checkExistUserName(option.value)
      changeMaxCredit(data.maxCredit);
    }
  };

  const clearSingleValue = () => {
    selectInputRef?.clearValue();
  };

  console.log("render search");
  return (
    <div>
      <AsyncCreatableSelect
        ref={(ref) => {
          selectInputRef = ref;
        }}
        components={{ Option: CustomOption }}
        placeholder=""
        styles={customerStyles}
        loadOptions={customPromiseOptions}
        onChange={handleOnChange}
        formatOptionLabel={(option) => {
          return option.userName ? option.userName : option["value"];
        }}
      />
       {isCorrectName ? <CheckMarkIcon height={16} width={16} /> : null}
    </div>
  );
};
