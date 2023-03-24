import React, { useState } from "react";
import dynamic from "next/dynamic";
import useTranslation from "next-translate/useTranslation";
import { Controller, useForm } from "react-hook-form";
import styles from "./NewCustomer.module.scss";
import { InformationIcon } from "@components/Icons";
import Message from "@components/Message";
import { User } from "src/lib/session";
import { numberAccounts } from "src/constants/number-accounts";
import { SingleSelect } from "@components/Form/Select";
import { Label } from "@components/Form/Label";
import BlockUI from "@components/BlockUI";
import Result from "./Result";
import { NumberInputField, PasswordField } from "@components/Form/Inputs";
import { formatThousandNumber, getNumberOfCommaString } from "src/lib/number";
import { validatePassword } from "src/lib/password";

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
  credit: string;
  memberMaxCredit?: string;
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
  user: User;
  minCredit: number;
  maxAccounts: number;
  actorName: string;
  isMasterLevel: boolean;
};

export const CopyCustomer = ({ ...props }: Props) => {
  const { t } = useTranslation("newcustomer");
  const methods = useForm<IFormInputs>({ mode: "onChange", defaultValues: { noOfAccounts: 1 }, shouldFocusError: false });
  const { handleSubmit, control, reset, formState } = methods;

  const [data, setData] = useState<Result>({});
  const [maxCredit, setMaxCredit] = useState(0);
  const [maxMemberCredit, setMaxMemberCredit] = useState(0);

  const onSubmit = handleSubmit(async (formData) => {
    console.log(formData);
    const url = `/api/customer/copy`;
    const result = await fetch(url, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        username: formData.username,
        password: formData.password,
        credit: getNumberOfCommaString(formData.credit),
        maxCredit: formData.memberMaxCredit && getNumberOfCommaString(formData.memberMaxCredit),
        sourceUserName: formData.existAccount,
        numberOfAccounts: Number(formData.noOfAccounts),
      }),
    });
    const data = await result.json();
    setData(data);
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
    reset();
  };

  // const renderErrors = (errors: FieldErrors) => {
  //   const errorKeys = ["existAccount", "username", "credit", "memberMaxCredit", "password"];
  //   let firstErr = "";
  //   for (const key of errorKeys) {
  //     if (errors[key]) {
  //       firstErr = key;
  //       break;
  //     }
  //   }
  //   if (firstErr !== "") {
  //     let message;
  //     if (firstErr === "credit") {
  //       message = t(`formError.${firstErr}`, { max: maxCredit.toFixed(2) });
  //     } else {
  //       message = t(`formError.${firstErr}`);
  //     }

  //     return <Message message={message}></Message>;
  //   }
  // };

  console.log("render copy customer");
  return (
    <>
      {formState.isSubmitting && <BlockUI />}
      {JSON.stringify(data) !== "{}" ? (
        <Result {...data} handleContinue={handleContinue} />
      ) : (
        <form className={styles["form-new-customer"]} onSubmit={onSubmit}>
          <p className={styles.note}>{t("note")}</p>

          {/* {renderErrors(formState.errors)} */}

          <Label title={t("sourceAccount")} required />
          <Controller
            name="existAccount"
            control={control}
            render={({ field }) => <SearchCustomerExists onChange={field.onChange} changeMaxCredit={handleChangeMaxCredit} />}
            rules={{ minLength: 1, required: true }}
          />

          <Label title={t("accountDetail")} description={t("accountDetailNote")} />
          <Label title={t("username")} />
          <Controller
            name="username"
            control={control}
            render={({ field }) => <NewAccountName actorName={props.actorName} onChange={field.onChange} />}
            rules={{ minLength: 1, required: true }}
          />

          <Label title={t("password")} subLabel={true} />
          <span title={t("formError.password")}>
            <InformationIcon height={16} width={16} />
          </span>
          <Controller
            name="password"
            control={control}
            render={({ field }) => <PasswordField onChange={field.onChange} validate={validatePassword} />}
            rules={{ minLength: 1, required: true }}
          />

          <Label title={t("noOfAccounts")} description={t("noAccountsNote", { max: props.maxAccounts })} />
          <Controller
            name="noOfAccounts"
            control={control}
            render={({ field: { onChange } }) => <SingleSelect options={numberAccounts} selectedValue={"1"} handleChange={onChange} />}
            rules={{ required: true }}
          />

          <Label title={`${t("credit")} ${t("perAccount")}`} subLabel={true} />
          <Controller
            name="credit"
            control={control}
            render={({ field }) => <NumberInputField onChange={field.onChange} onBlur={handleOnBlur} />}
            rules={{ min: 0, max: maxCredit, required: true }}
          />
          <div className={styles.credit}>
            {t("creditNote")}
            <span>{formatThousandNumber(maxCredit)}</span>
          </div>

          {props.isMasterLevel && (
            <>
              <Label title={`${t("memberMaxCredit")} ${t("perAccount")}`} subLabel={true} />
              <Controller
                name="memberMaxCredit"
                control={control}
                render={({ field }) => <NumberInputField onChange={field.onChange} />}
                rules={{ min: 0, max: maxMemberCredit, required: true }}
              />
              <div className={styles.credit}>
                <span>
                  =&lt;
                  {formatThousandNumber(maxMemberCredit)}
                </span>
              </div>
            </>
          )}
          <button className="btn-submit" type="submit" disabled={!formState.isValid}>
            {t("submit")}
          </button>
        </form>
      )}
    </>
  );
};
