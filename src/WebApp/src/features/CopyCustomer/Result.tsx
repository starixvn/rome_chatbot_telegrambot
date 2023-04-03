import Message from "@components/Message";
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
      {usernames && usernames.length > 0 ? (
        <>
          <Message message={`${usernames.length} ${t("result.success")}`} isError={false} />
          <p>{t("result.usernames")}</p>
          <div className={styles.info}>
            {usernames?.map((item, index) => {
              if (index === usernames.length - 1) {
                return <b key={item}>{item}</b>;
              }
              return <b key={item}>{item}, </b>;
            })}
          </div>
          <p>{t("result.credit")}</p>
          <input type="text" value={credit} />
        </>
      ) : null}
      {failAccounts && failAccounts > 0 ? (
        <>
          <Message message={`${failAccounts} ${t("result.fail")}`} isError />
          <span>{t("result.failReason")}: </span>
          <span className={styles.fail}>{message}</span>
        </>
      ) : null}
      <button className="btn-submit" onClick={handleContinue}>
        {t("result.createMore")}
      </button>
    </>
  );
};

export default Result;
