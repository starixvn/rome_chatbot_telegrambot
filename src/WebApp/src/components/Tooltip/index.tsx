import React from "react";
import { InformationIcon } from "@components/Icons";
import styles from "./Tooltip.module.scss";

type Position = "top" | "bottom";

type Props = {
  position: Position;
  message?: string;
};

export const Tooltip = ({ position, message = "" }: Props) => {
  return (
    <div className={styles.tooltip}>
      <span data-tooltip={message} data-tooltip-position={position}></span>
    </div>
  );
};
