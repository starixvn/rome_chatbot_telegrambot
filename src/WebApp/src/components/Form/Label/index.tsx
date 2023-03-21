import styles from "./Label.module.scss";

type Props = {
  title: string;
  subLabel?: boolean;
  description?: string;
  required?: boolean;
};

export const Label = ({ title, subLabel, description, required }: Props) => {
  return (
    <>
      <label className={subLabel ? styles["sub-label"] : ""}>{title}</label>
      {description && <p>{description}</p>}
      {required && <span className={styles.require}>*</span>}
    </>
  );
};
