import CryptoJS from "crypto-js";
import useTranslation from "next-translate/useTranslation";
import dynamic from "next/dynamic";
import { useRouter } from "next/router";
import React, { useState } from "react";
import { Controller, useForm } from "react-hook-form";
import { Agent, Master } from "src/constants/role-id";
import { generateAccountSelect } from "src/helpers/generate-account";
import { formatThousandNumber, getNumberOfCommaString } from "src/helpers/number";
import { validateMemberPassword, validatePassword } from "src/helpers/password";
import { fetchWrapper } from "src/lib/fetch-client";
import { User } from "src/lib/session";

import BlockUI from "@components/BlockUI";
import { CustomerCredit } from "@components/Customer/SearchCustomer";
import { NumberInputField, PasswordField } from "@components/Form/Inputs";
import { Label } from "@components/Form/Label";
import { SingleSelect } from "@components/Form/Select";
import { IconCredit } from "@components/Icons";
import Message, { MessageContent } from "@components/Message";
import { Tooltip } from "@components/Tooltip";

import styles from "./NewCustomer.module.scss";
import Result from "./Result";

const NewAccountName = dynamic(() => import("@components/Customer/NewAccount").then((mod) => mod.NewAccountName), {
  ssr: false,
});

const SearchCustomerExists = dynamic(() => import("@components/Customer/SearchCustomer").then((mod) => mod.SearchCustomerExists), {
  ssr: false,
});

type IFormInputs = {
  existAccount: string;
  noOfAccounts: number;
  username: string;
  credit: number;
  memberMaxCredit?: number;
  password: string;
};

type Result = {
  usernames?: string[];
  code?: number;
  message?: string;
  isSuccessful?: boolean;
  credit?: number;
  failAccounts?: number;
};

type Props = {
  minCredit: number;
  maxAccounts: number;
  user: User;
};

export const CopyCustomer = ({ user, ...props }: Props) => {
  const router = useRouter();
  const { t } = useTranslation("newcustomer");
  const methods = useForm<IFormInputs>({
    mode: "onSubmit",
    defaultValues: { noOfAccounts: 1, credit: 0, memberMaxCredit: user.customerLevel === Master ? 0 : undefined },
    shouldFocusError: false,
  });
  const { handleSubmit, control, reset, formState } = methods;

  const [data, setData] = useState<Result>({});
  const [errorMessage, setErrorMessage] = useState("");
  const [numberAccounts, setNumberAccounts] = useState(1);
  const [creditInfo, setCreditInfo] = useState<CustomerCredit | undefined>(undefined);
  const [maxMemberCredit, setMaxMemberCredit] = useState(0);
  const [existedSourceAccount, setExistedSourceAccount] = useState(false);
  const [messageContent, setMessageContent] = useState<MessageContent | undefined>(undefined);

  const onSubmit = handleSubmit(async (formData) => {
    const url = `/api/customer/copy`;
    const body = {
      username: formData.username,
      password: CryptoJS.AES.encrypt(formData.password, user.salt).toString(),
      credit: getNumberOfCommaString(formData.credit.toString()),
      memberMaxCredit: formData.memberMaxCredit && getNumberOfCommaString(formData.memberMaxCredit.toString()),
      sourceUserName: formData.existAccount,
      numberOfAccounts: Number(formData.noOfAccounts),
      notifyTelegramMessage: {
        success: t("result.success"),
        usernames: t("result.usernames"),
        givenCreditPerAccount: t("givenCreditPerAccount"),
        fail: t("result.fail"),
        failReason: t("result.failReason"),
      },
    };
    const data = await fetchWrapper.post<any>(url, body, router);
    if (data.usernames) {
      window &&
        window.scroll({
          top: 0,
          left: 0,
          behavior: "smooth",
        });

      if (data.usernames.length > 0) {
        setData(data);
      } else {
        if (data.messageContent) {
          setMessageContent(data.messageContent);
        } else {
          setErrorMessage(data.message);
        }
      }
    }
  });

  const handleOnBlur = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (getNumberOfCommaString(e.target.value) !== maxMemberCredit) {
      setMaxMemberCredit(getNumberOfCommaString(e.target.value));
    }
  };

  const handleContinue = () => {
    setData({});
    setCreditInfo(undefined);
    setMaxMemberCredit(0);
    setErrorMessage("");
    setMessageContent(undefined);
    reset();
  };

  const handlePasswordChange = (password: string) => {
    return user.customerLevel == Agent ? validateMemberPassword(password) : validatePassword(password);
  };

  const maxCredit = creditInfo?.maxCredit ?? 0;
  let maxGivenCreditPerAccount = maxCredit / numberAccounts;
  let creditHint = t("creditHint", { availableCredit: maxCredit });

  if (user.customerLevel === Agent && creditInfo?.memberMaxCredit) {
    creditHint = `${creditHint}<br>${t("memberMaxCreditHint", { memberMaxCredit: creditInfo.memberMaxCredit })}`;
    maxGivenCreditPerAccount = Math.min(maxGivenCreditPerAccount, creditInfo.memberMaxCredit);
  }

  return (
    <>
      {formState.isSubmitting && <BlockUI />}
      {JSON.stringify(data) !== "{}" ? (
        <Result {...data} handleContinue={handleContinue} />
      ) : (
        <form className={styles["form-new-customer"]} onSubmit={onSubmit}>
          <p className={styles.note}>
            <span>*</span> {t("note")}
          </p>

          {errorMessage && <Message message={errorMessage} isError></Message>}
          {messageContent && <MessageContent mainMessage={messageContent.mainMessage} subMessages={messageContent.subMessages} />}

          <Label title={t("sourceAccount")} required />
          <Controller
            name="existAccount"
            control={control}
            render={({ field }) => (
              <SearchCustomerExists onChange={field.onChange} setCreditInfo={setCreditInfo} setExistedSourceAccount={setExistedSourceAccount} />
            )}
            rules={{ minLength: 1, required: true }}
          />
          <hr />

          <Label title={t("accountDetail")} description={t("accountDetailNote")} />
          <Label title={t("username")} subLabel />
          <Controller
            name="username"
            control={control}
            render={({ field }) => <NewAccountName actorName={user.username} onChange={field.onChange} />}
            rules={{ minLength: 1, required: true }}
          />

          <Label title={t("password")} subLabel info tooltipMessage={t(user.customerLevel == Agent ? "memberPasswordRule" : "passwordRule")} />
          <Controller
            name="password"
            control={control}
            render={({ field }) => (
              <PasswordField onChange={field.onChange} validate={handlePasswordChange} maxLength={user.customerLevel == Agent ? 10 : 16} />
            )}
            rules={{ minLength: 1, required: true, validate: (value) => handlePasswordChange(value) }}
          />

          <Label title={t("noOfAccounts")} subLabel />
          <Controller
            name="noOfAccounts"
            control={control}
            render={({ field: { onChange } }) => (
              <div className={styles.row}>
                <SingleSelect
                  options={generateAccountSelect(props.maxAccounts)}
                  selectedValue={"1"}
                  handleChange={(e: React.ChangeEvent<HTMLSelectElement>) => {
                    setNumberAccounts(Number(e.target.value));
                    onChange(e);
                  }}
                />
              </div>
            )}
            rules={{ required: true, max: props.maxAccounts }}
          />

          <Label title={`${t("givenCreditPerAccount")}`} subLabel />
          <Controller
            name="credit"
            control={control}
            render={({ field }) => {
              return (
                <NumberInputField
                  onChange={(e) => {
                    field.onChange(e);
                    setMaxMemberCredit(getNumberOfCommaString(e.target.value));
                  }}
                  onBlur={handleOnBlur}
                  max={maxGivenCreditPerAccount}
                  placeholder="0"
                  messError={existedSourceAccount ? t("formError.credit", { max: formatThousandNumber(maxGivenCreditPerAccount) }) : ""}
                />
              );
            }}
            rules={{ min: 0, max: maxGivenCreditPerAccount, required: true }}
          />
          <div className={styles.credit}>
            {t("Min")}: <strong>0</strong> - {t("Max")}: <strong>{formatThousandNumber(maxGivenCreditPerAccount)}</strong>
            <div className={styles.info}>
              <IconCredit />
              <Tooltip message={creditHint} position="bottom" />
            </div>
          </div>

          {user.customerLevel === Master && (
            <>
              <Label title={`${t("maxCreditPerMember")}`} subLabel />
              <Controller
                name="memberMaxCredit"
                control={control}
                render={({ field }) => (
                  <NumberInputField
                    onChange={field.onChange}
                    max={maxMemberCredit}
                    placeholder="0"
                    messError={existedSourceAccount ? t("formError.memberMaxCredit", { min: 0, max: formatThousandNumber(maxMemberCredit) }) : ""}
                  />
                )}
                rules={{ min: 0, max: maxMemberCredit, required: true }}
              />
              <div className={styles.credit}>
                {t("Min")}: <strong>0</strong> - {t("Max")}: <strong>{formatThousandNumber(maxMemberCredit)}</strong>
              </div>
            </>
          )}
          <div className={styles.row}>
            <button className="btn-submit" type="submit" disabled={!formState.isValid}>
              {t("createAccount")}
            </button>
          </div>
        </form>
      )}
    </>
  );
};
