import useTranslation from "next-translate/useTranslation";
import React from "react";
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
  return (
    <>
      {usernames && usernames.length > 0 && (
        <>
          <div className={styles.message}>{t("result.success")}</div>
          <div className={styles.info}>
            <span>{t("result.usernames")} </span>
            {usernames?.map((item) => (
              <b key={item}>{item}, </b>
            ))}
          </div>
          <div className={styles.info}>
            <span>{t("result.credit", { credit: credit })}</span>
          </div>
        </>
      )}
      {failAccounts && failAccounts > 0 && (
        <>
          <div className={styles.message}>{t("result.fail", { failAccounts: failAccounts })}</div>
          <div className={styles.info}>
            <span>{t("result.failReason", { message: message })}</span>
          </div>
        </>
      )}
      <button className={styles["button-submit"]} onClick={handleContinue}>
        {t("result.createMore")}
      </button>
    </>
  );
};

export default Result;
