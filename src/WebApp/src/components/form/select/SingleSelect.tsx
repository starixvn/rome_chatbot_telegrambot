import { SelectHTMLAttributes } from "react";

export type SelectData = {
  value: number | string;
  displayName: string;
};

type Props = SelectHTMLAttributes<HTMLSelectElement> & {
  options: SelectData[];
  selectedValue: string;
  handleChange: (e: React.ChangeEvent<HTMLSelectElement>) => void;
};

export const SingleSelect = ({
  options,
  selectedValue,
  handleChange,
}: Props) => {
  return (
    <>
      <select defaultValue={selectedValue} onChange={handleChange}>
        {options.map((item) => (
          <option key={item.value} value={item.value}>
            {item.displayName}
          </option>
        ))}
      </select>
    </>
  );
};

SingleSelect.displayName = "SingleSelect";
