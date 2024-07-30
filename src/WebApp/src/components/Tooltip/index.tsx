import React from "react";
import styles from "./Tooltip.module.scss";

type Position = "top" | "bottom";

type Props = {
  position: Position;
  message?: string;
  children?: React.ReactNode;
};

export const Tooltip = ({ position, message = "", children }: Props) => {
  return (
    <div className={styles.tooltip}>
      <span data-tooltip={message} data-tooltip-position={position}>{children}</span>
    </div>
  );
};
