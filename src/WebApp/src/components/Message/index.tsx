import { ErrorIcon } from "@components/icons/Icons";
import React from "react";
import styles from "./message.module.scss";

const Message = ({ message }: { message: string; isError?: boolean }) => {
  return (
    <div className={`${styles.msg} ${styles["msg-error"]}`}>
      <ErrorIcon />
      <span className={styles["msg-content"]}>{message}</span>
    </div>
  );
};

export default Message;
