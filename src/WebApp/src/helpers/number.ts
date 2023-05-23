export const getNumberOfCommaString = (commaNumber: string) => {
  return Number(commaNumber.replaceAll(",", ""));
};

export const formatThousandNumber = (number: number) => {
  return number.toLocaleString("en-US", {
    minimumFractionDigits: 0,
    maximumFractionDigits: 2,
  });
};
