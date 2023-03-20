import { ErrorIcon } from "@components/Icons/Icons";
import React from "react";
import styles from "./message.module.scss";

const Message = ({ message }: { message: string; isError?: boolean }) => {
  return (
    <div className={`${styles.msg} ${styles["msg-error"]}`}>
      <ErrorIcon width={16} height={16} />
      <span className={styles["msg-content"]}>{message}</span>
    </div>
  );
};

export default Message;
