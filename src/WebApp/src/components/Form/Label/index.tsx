import cn from "classnames";
import styles from "./Label.module.scss";
import { Tooltip } from "@components/Tooltip";

type Props = {
  title: string;
  subLabel?: boolean;
  description?: string;
  required?: boolean;
  info?: boolean;
};

export const Label = ({ title, subLabel, description, required, info }: Props) => {
  return (
    <div className={styles.label}>
      <div className={styles.title}>
        <label className={cn({ [styles.subLabel]: subLabel })}>{title}</label>
        {required && <span className={styles.require}>*</span>}
        {info && <Tooltip />}
      </div>
      {description && <p>{description}</p>}
    </div>
  );
};
