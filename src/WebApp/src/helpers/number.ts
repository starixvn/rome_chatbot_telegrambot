export const getNumberOfCommaString = (commaNumber: string) => {
  return Number(commaNumber.replaceAll(",", ""));
};

export const formatThousandNumber = (number: number) => {
  return (Math.trunc(number * 100) / 100).toLocaleString("en-US");
};
