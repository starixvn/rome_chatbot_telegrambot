import cn from "classnames";
import styles from "./Label.module.scss";
import { Tooltip } from "@components/Tooltip";
import { InformationIcon } from "@components/Icons";

type Props = {
  title: string;
  subLabel?: boolean;
  description?: string;
  required?: boolean;
  info?: boolean;
  tooltipMessage?: string;
};

export const Label = ({ title, subLabel, description, required, info, tooltipMessage }: Props) => {
  return (
    <div className={styles.label}>
      <div className={styles.title}>
        <label className={cn({ [styles.subLabel]: subLabel })}>{title}</label>
        {required && <span className={styles.require}>*</span>}
        {info && (
          <div className={styles.info}>
            <InformationIcon height={16} width={16} />
            <Tooltip message={tooltipMessage} position="bottom" />
          </div>
        )}
      </div>
      {description && <p>{description}</p>}
    </div>
  );
};
