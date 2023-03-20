import { CheckMarkIcon } from "@components/icons/Icons";
import React, { useEffect, useState } from "react";
import { components, GroupBase, OptionProps } from "react-select";
import AsyncCreatableSelect from "react-select/async-creatable";
import Select from "react-select/dist/declarations/src/Select";
import { checkExistAccountName } from "../new-account/NewAccountName";
import { customerStyles } from "./CustomStyle";

const getExistCustomers = async (query: string) => {
  const url = `${process.env.NEXT_PUBLIC_CUSTOMER_SETTING_APP}/customer/search?query=${query}`;
  const res = await fetch(url);
  return await res.json();
};

const getCustomerMaxCredit = async (username: string) => {
  const url = `${process.env.NEXT_PUBLIC_CUSTOMER_SETTING_APP}/customer/credit/${username}`;
  const result = await fetch(url);
  return await result.json();
};

export const customPromiseOptions = async (inputValue: string) => {
  const data = await getExistCustomers(inputValue);
  return data.map((item: any) => {
    return {
      ...item,
      value: item.userName,
    };
  });
};

const CustomOption = (props: OptionProps<any>) => {
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
  let selectInputRef: Select<any, boolean, GroupBase<any>> | null = null;
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

  const onHandleChange = async (option: any) => {
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
        onChange={onHandleChange}
        formatOptionLabel={(option: any) => {
          return option.userName ? option.userName : option["value"];
        }}
      />
       {isCorrectName ? <CheckMarkIcon height={16} width={16} /> : null}
    </div>
  );
};
