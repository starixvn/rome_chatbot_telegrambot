export const getNumberOfCommaString = (commaNumber: string) => {
  return Number(commaNumber.replaceAll(",", ""));
};

export const formatThousandNumber = (number: number) => {
  return number.toLocaleString("en-US", {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
};
