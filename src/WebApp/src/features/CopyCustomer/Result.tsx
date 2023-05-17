import React, { useState } from "react";
import useTranslation from "next-translate/useTranslation";
import { CopyToClipboard } from "react-copy-to-clipboard";
import cn from "classnames";
import { CopiedICon, CopyICon } from "@components/Icons";
import Message from "@components/Message";
import styles from "./Result.module.scss";
import { Tooltip } from "@components/Tooltip";
import { formatThousandNumber } from "src/lib/number";

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
  const [isCopied, setIsCopied] = useState(false);

  const handleAllAccounts = () => {
    if (!isCopied) {
      setIsCopied(true);
      setTimeout(() => setIsCopied(false), 3000);
    }
  };

  return (
    <div className={styles.result}>
      <>
        <Message message={`${t("result.success", { no: usernames?.length })}`} isError={false} />
        <p>{t("result.usernames")}:</p>
        <div className={styles.copy}>
          <textarea rows={3} readOnly defaultValue={successAccounts}></textarea>
          <CopyToClipboard text={successAccounts || ""}>
            <div className={cn(styles.icon, { [styles.show]: isCopied })} onClick={handleAllAccounts}>
              <div className={styles["icon-copy"]}>{isCopied ? <CopiedICon width={16} height={16} /> : <CopyICon width={16} height={16} />}</div>
              <Tooltip message={t("copied")} position="top" />
            </div>
          </CopyToClipboard>
        </div>
        <p>{t("givenCreditPerAccount")}</p>
        <input value={formatThousandNumber(Number(credit))} readOnly />
      </>
      {failAccounts && failAccounts > 0 ? (
        <>
          <hr />
          <Message message={`${t("result.fail", { failAccounts: failAccounts })}`} isError />
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
