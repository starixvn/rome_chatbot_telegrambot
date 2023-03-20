import { CustomerInfoOption } from './SearchCustomerExists';
import { StylesConfig } from "react-select";

export const customerStyles: StylesConfig<CustomerInfoOption> = {
  control: (baseStyles) => ({
    ...baseStyles,
    minHeight: "30px",
    height: "30px",
    width: "320px",
    display: "inline-block",
    paddingLeft: "3px",
  }),
  
  container: (provided) => ({
    ...provided,
    display: "inline-block",
  }),

  valueContainer: (provided) => ({
    ...provided,
    height: "30px",
    padding: "0 6px",
  }),

  input: (provided) => ({
    ...provided,
    margin: "0px 2px",
  }),

  indicatorSeparator: () => ({
    display: "none",
  }),

  indicatorsContainer: (provided) => ({
    ...provided,
    display: "none",
  }),
};
