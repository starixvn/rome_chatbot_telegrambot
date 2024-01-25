import cn from "classnames";
import { CheckMarkIcon, ErrorIcon, InformationIcon } from "@components/Icons";
import useTranslation from "next-translate/useTranslation";
import React from "react";

import styles from "./message.module.scss";

const Message = ({ message, isError, isWarning }: { message: string; isError?: boolean; isWarning?: boolean }) => {
  return (
    <>
      {
        isError && <div className={cn([styles.msg], { [styles.msgError]: true })}>
            <ErrorIcon width={16} height={16} />
            <span className={styles["msg-content"]}>{message}</span>
          </div>
      }
      {
          isWarning && <div className={cn([styles.msg], { [styles.msgWarning]: true })}>
              <InformationIcon height={16} width={16} />
              <span className={styles["msg-content"]}>{message}</span>
            </div>
      }
      {
          !isError && !isWarning && <div className={cn([styles.msg], { [styles.msgSuccess]: true })}>
              <CheckMarkIcon height={16} width={16} />
              <span className={styles["msg-content"]}>{message}</span>
          </div>
      }
    </>
  );
};

export type SubMessage = {
  message: string;
  isLive: boolean;
};

export type MessageContent = {
  mainMessage: string;
  subMessages: SubMessage[];
};

export const MessageContent = ({ mainMessage, subMessages }: MessageContent) => {
  const { t } = useTranslation();
  return (
    <div className={cn([styles.msg, styles.msgError])}>
      <ErrorIcon width={16} height={16} />
      <span className={styles["msg-content"]}>
        <div>{mainMessage}</div>
        {subMessages &&
          subMessages.length &&
          subMessages.map((subMessage, index) => (
            <div key={index}>
              {subMessage.isLive && <span className={styles["live-badge"]}>{t("Live")}</span>}
              {subMessage.message}
            </div>
          ))}
      </span>
    </div>
  );
};

export default Message;
