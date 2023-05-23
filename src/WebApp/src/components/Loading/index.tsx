import React from "react";
import useTranslation from "next-translate/useTranslation";
import styles from "./Loading.module.scss";

const Loading = () => {
  const { t } = useTranslation("common");
  return (
    <div className={styles.loading}>
      <div className={styles["lds-spinner"]}>
        <div></div>
        <div></div>
        <div></div>
        <div></div>
        <div></div>
        <div></div>
        <div></div>
        <div></div>
        <div></div>
        <div></div>
        <div></div>
        <div></div>
      </div>
      <div className={styles.content}>
        <p>{t("Loading")}...</p>
      </div>
    </div>
  );
};

export default Loading;
