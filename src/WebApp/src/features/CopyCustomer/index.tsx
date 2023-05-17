import React, { useState } from "react";
import dynamic from "next/dynamic";
import useTranslation from "next-translate/useTranslation";
import { Controller, useForm } from "react-hook-form";
import Message from "@components/Message";
import { generateAccountSelect } from "src/constants/number-accounts";
import { SingleSelect } from "@components/Form/Select";
import { Label } from "@components/Form/Label";
import BlockUI from "@components/BlockUI";
import Result from "./Result";
import { NumberInputField, PasswordField } from "@components/Form/Inputs";
import { formatThousandNumber, getNumberOfCommaString } from "src/lib/number";
import { validatePassword } from "src/lib/password";
import { fetchWrapper } from "src/lib/fetch-client";
import { User } from "src/lib/session";
import { Master } from "src/constants/role-id";
import styles from "./NewCustomer.module.scss";
import { useRouter } from "next/router";
import CryptoJS from "crypto-js";

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
  const [maxCredit, setMaxCredit] = useState(0);
  const [maxMemberCredit, setMaxMemberCredit] = useState(0);

  const onSubmit = handleSubmit(async (formData) => {
    const url = `/api/customer/copy`;
    const body = {
      username: formData.username,
      password: CryptoJS.AES.encrypt(formData.password, user.salt).toString(),
      credit: getNumberOfCommaString(formData.credit.toString()),
      maxCredit: formData.memberMaxCredit && getNumberOfCommaString(formData.memberMaxCredit.toString()),
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
    if (data.usernames.length > 0) {
      setData(data);
    } else {
      setErrorMessage(data.message);
    }
  });

  const handleChangeMaxCredit = (value: number) => {
    setMaxCredit(value);
  };

  const handleOnBlur = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (getNumberOfCommaString(e.target.value) !== maxMemberCredit) {
      setMaxMemberCredit(getNumberOfCommaString(e.target.value));
    }
  };

  const handleContinue = () => {
    setData({});
    setMaxCredit(0);
    setMaxMemberCredit(0);
    setErrorMessage("");
    reset();
  };

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

          <Label title={t("sourceAccount")} required />
          <Controller
            name="existAccount"
            control={control}
            render={({ field }) => <SearchCustomerExists onChange={field.onChange} changeMaxCredit={handleChangeMaxCredit} />}
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

          <Label title={t("password")} subLabel info tooltipMessage={t("passwordRule")} />
          <Controller
            name="password"
            control={control}
            render={({ field }) => <PasswordField onChange={field.onChange} validate={validatePassword} />}
            rules={{ minLength: 1, required: true, validate: (value) => validatePassword(value) }}
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
            render={({ field }) => (
              <NumberInputField
                onChange={(e) => {
                  field.onChange(e);
                  setMaxMemberCredit(getNumberOfCommaString(e.target.value));
                }}
                onBlur={handleOnBlur}
                max={maxCredit / numberAccounts}
                placeholder="0"
                messError={t("formError.credit", { max: formatThousandNumber(Number((maxCredit / numberAccounts).toFixed(2))) })}
              />
            )}
            rules={{ min: 0, max: maxCredit / numberAccounts, required: true }}
          />
          <div className={styles.credit}>
            {t("creditNote")}
            <span>{formatThousandNumber(maxCredit)}</span>
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
                    messError={t("formError.memberMaxCredit", { min: 0, max: formatThousandNumber(maxMemberCredit) })}
                  />
                )}
                rules={{ min: 0, max: maxMemberCredit, required: true }}
              />
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
