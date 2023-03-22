import React, { useState } from "react";
import dynamic from "next/dynamic";
import useTranslation from "next-translate/useTranslation";
import { Controller, FieldErrors, useForm } from "react-hook-form";
import styles from "./NewCustomer.module.scss";
import { InformationIcon, EyeIcon, EyeCloseIcon, CheckMarkIcon, ErrorIcon } from "@components/Icons";
import Message from "@components/Message";
import { User } from "src/lib/session";
import { numberAccounts } from "src/constants/number-accounts";
import { SingleSelect } from "@components/Form/Select";
import { Label } from "@components/Form/Label";
import BlockUI from "@components/BlockUI";
import Result from "./Result";

const NewAccountName = dynamic(() => import("@components/Customer/NewAccount").then((mod) => mod.NewAccountName), {
  ssr: false,
});

const SearchCustomerExists = dynamic(() => import("@components/Customer/SearchCustomer").then((mod) => mod.SearchCustomerExists), {
  ssr: false,
});

const validatePassword = (password: string) => {
  const whiteSpace = /^\S*$/;
  const whiteSpaceCase = password.search(whiteSpace) != -1;

  const regex1 = /^(?=.{8,100}$).*\d/i;
  const regex2 = /^(?=.{8,100}$).*[a-z]/;
  const regex3 = /^(?=.{8,100}$).*[A-Z]/;
  const regex4 = /^(?=.{8,100}$).*[\[\]\^\$\.\|\?\*\+\(\)\\~`\!@#%&\-_+={}'""<>:;,]/i;

  const case1 = password.search(regex1) != -1 ? 1 : 0;
  const case2 = password.search(regex2) != -1 ? 1 : 0;
  const case3 = password.search(regex3) != -1 ? 1 : 0;
  const case4 = password.search(regex4) != -1 ? 1 : 0;

  return case1 + case2 + case3 + case4 >= 3 && whiteSpaceCase;
};

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
  user: User;
  minCredit: number;
  maxAccounts: number;
  actorName: string;
  isMasterLevel: boolean;
};

export const CopyCustomer = ({ ...props }: Props) => {
  const methods = useForm<IFormInputs>({ defaultValues: { noOfAccounts: 1 }, shouldFocusError: false });
  const { t } = useTranslation("newcustomer");
  const {
    register,
    handleSubmit,
    control,
    reset,
    formState: { errors, isSubmitting },
    resetField,
    getValues,
  } = methods;

  const [data, setData] = useState<Result>({});
  const [maxCredit, setMaxCredit] = useState(0);
  const [maxMemberCredit, setMaxMemberCredit] = useState(0);
  const [passwordShown, setPasswordShown] = useState(false);
  const [isPasswordValid, setIsPasswordValid] = useState<boolean | undefined>(undefined);

  const handleShowPassword = () => {
    setPasswordShown(!passwordShown);
  };

  const handlePasswordKeyup = () => {
    const password = getValues("password");
    const isValid = validatePassword(password);
    setIsPasswordValid(isValid);
  };

  const onSubmit = handleSubmit(async (formData) => {
    const url = `/api/customer/copy`;
    const result = await fetch(url, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        username: formData.username,
        password: formData.password,
        credit: Number(formData.credit),
        maxCredit: Number(formData.memberMaxCredit) || 0,
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
    if (Number(e.target.value) !== maxMemberCredit) {
      setMaxMemberCredit(Number(e.target.value));
    }
  };

  const handleContinue = () => {
    setData({});
    setMaxCredit(0);
    setMaxMemberCredit(0);
    resetField("credit");
    reset();
  };

  const renderErrors = (errors: FieldErrors) => {
    const errorKeys = ["existAccount", "username", "credit", "memberMaxCredit", "password"];
    let firstErr = "";
    for (const key of errorKeys) {
      if (errors[key]) {
        firstErr = key;
        break;
      }
    }
    if (firstErr !== "") {
      let message;
      if (firstErr === "credit") {
        message = t(`formError.${firstErr}`, { max: maxCredit.toFixed(2) });
      } else {
        message = t(`formError.${firstErr}`);
      }

      return <Message message={message}></Message>;
    }
  };

  return (
    <>
      {isSubmitting && <BlockUI />}
      {JSON.stringify(data) !== "{}" ? (
        <Result {...data} handleContinue={handleContinue} />
      ) : (
        <form className={styles["form-new-customer"]} onSubmit={onSubmit}>
          <p className={styles.note}>{t("note")}</p>
          {renderErrors(errors)}
          <Label title={t("copySettings")} description={t("searchNote")} />
          <div className={styles.formcontrolSetting}>
            <Controller
              name="existAccount"
              control={control}
              render={({ field: { onChange } }) => <SearchCustomerExists onChange={onChange} changeMaxCredit={handleChangeMaxCredit} />}
              rules={{ minLength: 1, required: true }}
            />
          </div>
          <Label title={t("noOfAccounts")} description={t("noAccountsNote", { max: props.maxAccounts })} />
          <div className={styles.formcontrolSelectFull}>
            <Controller
              name="noOfAccounts"
              control={control}
              render={({ field: { onChange } }) => <SingleSelect options={numberAccounts} selectedValue={"1"} handleChange={onChange} />}
              rules={{ required: true }}
            />
          </div>
          <Label title={t("accountDetail")} description={t("accountDetailNote")} />
          <Label title={t("username")} required={true} />
          <div className={styles.agent}>
            <Controller
              name="username"
              control={control}
              render={({ field: { onChange } }) => <NewAccountName actorName={props.actorName} onChange={onChange} />}
              rules={{ minLength: 1, required: true }}
            />
          </div>
          <Label title={t("credit")} subLabel={true} required={true} />
          <div className={styles.peraccount}>
            <input
              type="number"
              className={styles.control + " " + styles.full}
              {...register("credit", {
                min: 0,
                max: maxCredit,
                required: true,
              })}
              onBlur={handleOnBlur}
            />
            <span>{t("perAccount")}</span>
          </div>
          <div className={styles.credit}>
            <span>{props.minCredit.toFixed(2)}</span>
            {t("creditNote")}
            <span>{maxCredit.toFixed(2)}</span>
          </div>
          {props.isMasterLevel && (
            <div>
              <Label title={t("memberMaxCredit")} subLabel={true} required={true} />
              <div className={styles.peraccount}>
                <input
                  type="number"
                  {...register("memberMaxCredit", {
                    required: true,
                    max: maxMemberCredit,
                  })}
                />
                <span>{t("perAccount")}</span>
              </div>
              <div className={styles.credit}>
                <span>=&lt; {maxMemberCredit.toFixed(2)}</span>
              </div>
            </div>
          )}
          <div className={styles["has-info"]}>
            <Label title={t("password")} subLabel={true} required={true} />
            <span title={t("formError.password")}>
              <InformationIcon height={16} width={16} />
            </span>
          </div>
          <div className={styles["d-flex"] + " " + styles["align-items-center"]}>
            <div className={styles["relative"] + " " + styles["w-320"]}>
              <input
                className={styles.control + " " + styles.full}
                {...register("password", {
                  required: true,
                  validate: validatePassword,
                })}
                type={passwordShown ? "text" : "password"}
                onKeyUp={handlePasswordKeyup}
              />
              <span className={styles.showpassword} onClick={handleShowPassword}>
                {passwordShown ? <EyeIcon /> : <EyeCloseIcon />}
              </span>
            </div>
            {isPasswordValid !== undefined && (
              <div className={styles["icon"]}>{isPasswordValid ? <CheckMarkIcon height={16} width={16} /> : <ErrorIcon height={16} width={16} />}</div>
            )}
          </div>
          <button className="btn-submit" type="submit">
            {t("submit")}
          </button>
        </form>
      )}
    </>
  );
};
