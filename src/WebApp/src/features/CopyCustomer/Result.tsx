import { CheckMarkIcon } from "@components/Icons";
import Message from "@components/Message";
import useTranslation from "next-translate/useTranslation";
import React from "react";
import { NumericFormat } from "react-number-format";
import styles from "./Result.module.scss";

type Props = {
  usernames?: string[];
  message?: string;
  credit?: number;
  failAccounts?: number;
  handleContinue?: () => void;
};

const Result: React.FC<Props> = ({ usernames, failAccounts, credit, message, handleContinue }) => {
  const { t } = useTranslation("newcustomer");
  const successAccounts = usernames?.join(", ");

  return (
    <div className={styles.result}>
      <>
        <Message message={`${usernames?.length} ${t("result.success")}`} isError={false} />
        <p>{t("result.usernames")}</p>
        <div className={styles.copy}>
          <textarea rows={3} readOnly>
            {`${successAccounts}.`}
          </textarea>
          <div className={styles.icon}>
            <CheckMarkIcon width={16} height={16} />
          </div>
        </div>
        <p>{t("result.credit")}</p>
        <input value={credit?.toFixed(2)} />
      </>
      {failAccounts && failAccounts > 0 ? (
        <>
          <Message message={`${failAccounts} ${t("result.fail")}`} isError />
          <div className={styles.row}>
            <p>{t("result.failReason")}: </p>
            <span className={styles.fail}>{message}</span>
          </div>
        </>
      ) : null}
      <button className="btn-submit" onClick={handleContinue}>
        {t("result.createMore")}
      </button>
    </div>
  );
};

export default Result;
