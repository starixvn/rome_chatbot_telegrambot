import { SelectHTMLAttributes } from "react";
import styles from "./Select.module.scss";

export type SelectData = {
  value: number | string;
  displayName: string;
};

type Props = SelectHTMLAttributes<HTMLSelectElement> & {
  options: SelectData[];
  selectedValue: string;
  handleChange: (e: React.ChangeEvent<HTMLSelectElement>) => void;
};

export const SingleSelect = ({ options, selectedValue, handleChange }: Props) => {
  return (
    <>
      <select className={styles.select} defaultValue={selectedValue} onChange={handleChange}>
        {options.map((item: SelectData) => (
          <option key={item.value} value={item.value}>
            {item.displayName}
          </option>
        ))}
      </select>
    </>
  );
};

SingleSelect.displayName = "SingleSelect";
