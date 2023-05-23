import { CheckMarkIcon, ErrorIcon } from "@components/Icons";
import React from "react";
import styles from "./message.module.scss";
import cn from "classnames";

const Message = ({ message, isError }: { message: string; isError?: boolean }) => {
  return (
    <div className={cn([styles.msg], { [styles.msgError]: isError }, { [styles.msgSuccess]: !isError })}>
      {isError ? <ErrorIcon width={16} height={16} /> : <CheckMarkIcon height={16} width={16} />}
      <span className={styles["msg-content"]}>{message}</span>
    </div>
  );
};

export default Message;
