import React, { useState } from "react";
import useTranslation from "next-translate/useTranslation";
import { CopyToClipboard } from "react-copy-to-clipboard";
import cn from "classnames";
import { CopiedICon, CopyICon } from "@components/Icons";
import Message from "@components/Message";
import styles from "./Result.module.scss";
import { Tooltip } from "@components/Tooltip";
import { formatThousandNumber } from "src/helpers/number";

type Props = {
  usernames?: string[];
  domains?: string[];
  message?: string;
  credit?: number;
  failAccounts?: number;
  handleContinue?: () => void;
};
const formatDomainList=(domains: string[] | undefined)=> {
  if (!domains) {
    return "";
  }

  return domains.length < 4
    ? domains.join(', ')
    : `${domains.slice(0, 3).join(', ')}`;
}

function CopyToClipboardV2(text: string): Promise<boolean> {
  return new Promise((resolve, reject) => {
    if (window.navigator["clipboard"] && window.isSecureContext) {
      window.navigator["clipboard"].writeText(text)
        .then(() => resolve(true))  // Resolve on success
        .catch(err => {
          console.error("Navigator Clipboard API error:", err);
          reject(false);  // Reject on error
        });
    } else {
      const textArea = document.createElement("textarea");
      textArea.value = text;
      textArea.style.display = "none";

      document.body.prepend(textArea);
      textArea.select();

      try {
        document.execCommand('copy');
        resolve(true);  // Resolve on success with fallback method
      } catch (error) {
        console.error("Fallback copy error:", error);
        reject(false);  // Reject on error
      } finally {
        textArea.remove();
      }
    }
  });
}

const Result: React.FC<Props> = ({ usernames,domains, failAccounts, credit, message, handleContinue }) => {
  const { t } = useTranslation("newcustomer");
  const successAccounts = usernames?.join(", ");
  const formatCredit = formatThousandNumber(Number(credit));
  const formattedDomains = formatDomainList(domains);
  const [isCopied, setIsCopied] = useState(false);

  const handleAllAccounts = () => {
    if (!isCopied) {
      setIsCopied(true);
      setTimeout(() => setIsCopied(false), 3000);
    }
  };
  const userInfo = (username: string , credit: string, domains: string) => {
    return `${t("username")}: ${username}\n${t("credit")}: ${credit}\n${t("domain")}: ${domains}\n`;
  }

  return (
    <div className={styles.result}>
      <>
        <Message message={`${t("result.success", { no: usernames?.length })}`} isError={false} />
        <div><span style={{color: "red"}}>*</span> {t("result.introCopyFeature")}</div>

        <button onClick={()=>CopyToClipboardV2(userInfo(usernames?.join(", ") || "", formatCredit, formattedDomains))}>
          {t("result.compactCopy")}
        </button>
        <button onClick={()=>CopyToClipboardV2(usernames?.map(name => userInfo(name, formatCredit, formattedDomains)).join("\n") || "")}>
          {t("result.individualCopy")}
        </button>

        <p>{t("result.usernames")}:</p>
        <div className={styles.copy}>
          <textarea rows={3} readOnly defaultValue={successAccounts}></textarea>
        </div>

        <p>{t("givenCreditPerAccount")}:</p>
        <input value={formatCredit} readOnly />

        <p>{t("domain")}:</p>
        <textarea rows={3} readOnly defaultValue={formattedDomains}></textarea>
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
