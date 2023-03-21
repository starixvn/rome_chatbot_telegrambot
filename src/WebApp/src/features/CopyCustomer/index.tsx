import React, { useState } from "react";
import dynamic from "next/dynamic";
import useTranslation from "next-translate/useTranslation";
import { Controller, FieldErrors, useForm } from "react-hook-form";
import styles from "./NewCustomer.module.scss";
import { InformationIcon, EyeIcon } from "@components/Icons";
import Message from "@components/Message";
import { User } from "src/lib/session";
import { numberAccounts } from "src/constants/number-accounts";
import { SingleSelect } from "@components/Form/Select";

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
    formState: { errors },
    resetField,
  } = methods;

  const [data, setData] = useState<Result>({});
  const [maxCredit, setMaxCredit] = useState(0);
  const [maxMemberCredit, setMaxMemberCredit] = useState(0);

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
    resetField("credit");
    reset();
  };

  const renderResult = (data: Result) => {
    return (
      <>
        {data?.usernames && data?.usernames.length > 0 ? (
          <>
            <div className={styles["message"]}>{t("result.success")}</div>
            <div className={styles["infor"]}>
              <span>{t("result.usernames")} </span>
              {data.usernames?.map((item) => (
                <b key={item}>{item}, </b>
              ))}
            </div>
            <div className={styles["infor"]}>
              <span>{t("result.credit", { credit: data.credit })}</span>
            </div>
          </>
        ) : null}
        {data?.failAccounts && data.failAccounts > 0 ? (
          <>
            <div className={styles["message"]}>{t("result.fail", { failAccounts: data.failAccounts })}</div>
            <div className={styles["infor"]}>
              <span>{t("result.failReason", { message: data.message })}</span>
            </div>
          </>
        ) : null}
        <button className={styles["button-submit"]} onClick={handleContinue}>
          {t("result.createMore")}
        </button>
      </>
    );
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

  console.log("create new customer");
  return (
    <>
      {JSON.stringify(data) !== "{}" ? (
        renderResult(data)
      ) : (
        <div className={styles.main}>
          <p className={styles.note + " " + styles.notetop}>{t("note")}</p>
          <form onSubmit={onSubmit}>
            {renderErrors(errors)}
            <label className={styles.text}>{t("copySettings")}</label>
            <p className={styles.text}> {t("searchNote")}</p>
            <div className={styles.formcontrolSetting}>
              <Controller
                name="existAccount"
                control={control}
                render={({ field: { onChange } }) => <SearchCustomerExists onChange={onChange} changeMaxCredit={handleChangeMaxCredit} />}
                rules={{ minLength: 1, required: true }}
              />
            </div>
            <label className={styles.text}>{t("noOfAccounts")}</label>
            <p className={styles.text}>{t("noAccountsNote", { max: props.maxAccounts })}</p>
            <div className={styles.formcontrolSelectFull}>
              <Controller
                name="noOfAccounts"
                control={control}
                render={({ field: { onChange } }) => <SingleSelect options={numberAccounts} selectedValue={"1"} handleChange={onChange} />}
                rules={{ required: true }}
              />
            </div>
            <label className={styles.text}>{t("accountDetail")}</label>
            <p className={styles.text}> {t("accountDetailNote")}</p>
            <label className={styles.sublabel}>
              {t("username")}
              <span className={styles.require}>*</span>
            </label>
            <div className={styles.agent}>
              <Controller
                name="username"
                control={control}
                render={({ field: { onChange } }) => <NewAccountName actorName={props.actorName} onChange={onChange} />}
                rules={{ minLength: 1, required: true }}
              />
            </div>
            <label className={styles.sublabel}>
              {t("credit")}
              <span className={styles.require}>*</span>
            </label>
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
            {props.isMasterLevel ? (
              <div>
                <label className={styles.sublabel}>
                  {t("memberMaxCredit")}
                  <span className={styles.require}>*</span>
                </label>
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
            ) : null}
            <label className={styles.sublabel}>
              {t("password")}
              <span className={styles.require}>*</span>
              <InformationIcon height={16} width={16} />
            </label>
            <div className={styles.relative}>
              <input
                className={styles.control + " " + styles.full}
                {...register("password", {
                  required: true,
                  validate: validatePassword,
                })}
                type="password"
              />
              <span className={styles.showpassword}>
                <EyeIcon height={16} width={16} />
              </span>
            </div>
            <button className={styles["button-submit"]} type="submit">
              Submit
            </button>
          </form>
        </div>
      )}
    </>
  );
};
