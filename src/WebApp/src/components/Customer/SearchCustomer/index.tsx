/* eslint-disable react-hooks/exhaustive-deps */
import React, { useEffect, useState } from "react";
import useTranslation from "next-translate/useTranslation";
import AsyncSelect from "react-select/async";
import { components, GroupBase, InputActionMeta, OptionProps, SingleValue, StylesConfig } from "react-select";
import { fetchWrapper } from "src/lib/fetch-wrapper";
import { CheckMarkIcon, ErrorIcon } from "@components/Icons";
import Select from "react-select/dist/declarations/src/Select";
import Message from "@components/Message";
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

export const customPromiseOptions = async (inputValue: string) => {
  const data = await fetchWrapper.get(`/api/customer/search?query=${inputValue}`);
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

const Input = (props: any) => <components.Input {...props} isHidden={false} />;

type Props = {
  onChange: (existAccountName: string) => void;
  changeMaxCredit: (maxCredit: number) => void;
};

export const SearchCustomerExists = ({ onChange, changeMaxCredit }: Props) => {
  const { t } = useTranslation("newcustomer");
  let selectInputRef: Select<CustomerInfoOption, boolean, GroupBase<CustomerInfoOption>> | null = null;
  const [isCorrectName, setIsCorrectName] = useState<ErrorMode>(ErrorMode.Init);
  const [value, setValue] = useState("");
  const [showError, setShowError] = useState(false);

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
    const data = await fetchWrapper.get(`/api/customer/downline/${username}`);
    if (data.isDownLineUser) {
      onChange(username);
      setIsCorrectName(ErrorMode.Correct);
      setShowError(false);
      const data = await fetchWrapper.get(`/api/customer/credit/${username}`);
      changeMaxCredit(data.maxCredit);
    } else {
      onChange("");
      setIsCorrectName(ErrorMode.InCorrect);
      setShowError(true);
    }
  };

  const clearSingleValue = () => {
    selectInputRef?.clearValue();
  };

  const onFocus = () => {
    selectInputRef?.inputRef?.setSelectionRange(value.length, value.length);
    value && selectInputRef?.inputRef?.focus();
  };

  return (
    <div className={styles.container}>
      <div className={styles.search}>
        <AsyncSelect
          ref={(ref) => {
            selectInputRef = ref;
          }}
          inputValue={value}
          onInputChange={onInputChange}
          components={{ Option: CustomOption, Input: Input, SingleValue: () => null }}
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
          onFocus={onFocus}
          onBlur={() => setShowError(false)}
          noOptionsMessage={() => null}
          controlShouldRenderValue={false}
          formatOptionLabel={(option) => {
            return option.userName ? option.userName : option["value"];
          }}
          menuIsOpen={true}
        />
        <div className={styles.icon}>
          {isCorrectName === ErrorMode.Correct && <CheckMarkIcon height={16} width={16} />}
          {isCorrectName === ErrorMode.InCorrect && <ErrorIcon height={16} width={16} />}
        </div>
      </div>
      {showError && (
        <div className={styles.row}>
          <Message message={t("formError.existAccount")} isError />
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
    paddingLeft: "5px",
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
