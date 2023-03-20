export const customerStyles = {
  control: (baseStyles: any, state: any) => ({
    ...baseStyles,
    minHeight: "30px",
    height: "30px",
    width: "320px",
    display: "inline-block",
    paddingLeft: "3px",
  }),
  container: (provided: any, state: any) => ({
    ...provided,
    display: "inline-block",
  }),

  valueContainer: (provided: any, state: any) => ({
    ...provided,
    height: "30px",
    padding: "0 6px",
  }),

  input: (provided: any, state: any) => ({
    ...provided,
    margin: "0px 2px",
  }),

  indicatorSeparator: (state: any) => ({
    display: "none",
  }),

  indicatorsContainer: (provided: any, state: any) => ({
    ...provided,
    display: "none",
  }),
};
