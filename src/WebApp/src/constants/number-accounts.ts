import { SelectData } from "@components/Form/Select";

export const generateAccountSelect = (maxAccount: number) => {
  return Array.from({ length: maxAccount }, (_, i) => i + 1).map(
    (number) =>
      ({
        value: number,
        displayName: number.toString(),
      } as SelectData)
  );
};
