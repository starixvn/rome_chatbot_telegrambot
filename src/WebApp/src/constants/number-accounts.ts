import { SelectData } from "@components/Form/Select";

const noAccounts = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

export const numberAccounts: SelectData[] = noAccounts.map((number) => ({
  value: number,
  displayName: number.toString(),
}));
