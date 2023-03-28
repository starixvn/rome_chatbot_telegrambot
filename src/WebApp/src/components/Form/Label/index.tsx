import { InformationIcon } from "@components/Icons";
import cn from "classnames";
import styles from "./Label.module.scss";

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
        {info && <InformationIcon height={16} width={16} />}
      </div>
      {description && <p>{description}</p>}
    </div>
  );
};
