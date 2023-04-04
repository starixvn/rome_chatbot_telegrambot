import React from "react";
import { InformationIcon } from "@components/Icons";
import styles from "./Tooltip.module.scss";
import useTranslation from "next-translate/useTranslation";

export const Tooltip = () => {
  const { t } = useTranslation("newcustomer");
  return (
    <div className={styles.tooltip}>
      <span data-tooltip={t("formError.password")} data-tooltip-position="bottom">
        <InformationIcon height={16} width={16} />
      </span>
    </div>
  );
};
