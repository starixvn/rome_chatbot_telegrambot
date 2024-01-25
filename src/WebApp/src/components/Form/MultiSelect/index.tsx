import styles from "./MultiSelect.module.scss";
import { useState, useRef } from "react";
import { default as ReactSelect, components, InputAction } from "react-select";
import useTranslation from "next-translate/useTranslation";

export type Option = {
  value: number | string;
  label: string;
}; 

type Props = {
  options: any;
  value: any;
  onChange: any;
  isSelectAll: any;
  menuPlacement: any;
  components?: any;
  styles?: any;
};

const MultiSelect = (props: Props) => {
  const { t } = useTranslation("multiselection");

  const [selectInput, setSelectInput] = useState<string>("");
  const isAllSelected = useRef<boolean>(props.isSelectAll);
  const selectAllLabel = useRef<string>(t("selectAll"));
  const allOption = { value: "*", label: selectAllLabel.current };

  const isTextMatch = (label: string, filter: string) => label.toLowerCase().includes(filter?.toLowerCase());
  const filterOptions = (options: Option[], input: string) => options?.filter(({ label }: Option) => isTextMatch(label, input));
  const comparator = (v1: Option, v2: Option) => (v1.value as number) - (v2.value as number);
  const areObjectsEqual = (ob1: any, ob2: any) => JSON.stringify(ob1) === JSON.stringify(ob2);

  const filteredOptions = filterOptions(props.options, selectInput);
  const filteredSelectedOptions = filterOptions(props.value, selectInput);

  const Option = (props: any) => (
    <components.Option {...props}>
      {props.value === "*" &&
      !isAllSelected.current &&
      filteredSelectedOptions?.length > 0 ? (
        <input
          key={props.value}
          type="checkbox"
          className={styles["chk-status"]}
          ref={(input) => {
            if (input) input.indeterminate = true;
          }}
        />
      ) : (
        <input
          key={props.value}
          className={styles["chk-status"]}
          type="checkbox"
          checked={props.isSelected || isAllSelected.current}
        />
      )}
      <label style={{ marginLeft: "5px" }}>{props.label}</label>
    </components.Option>
  );

  const Input = (props: any) => (
    <>
      {selectInput.length === 0 ? (
        <components.Input autoFocus={props.selectProps.menuIsOpen} {...props}>
          {props.children}
        </components.Input>
      ) : (
        <div style={{ border: "1px dotted gray", height: "26px" }}>
          <components.Input autoFocus={props.selectProps.menuIsOpen} {...props}>
            {props.children}
          </components.Input>
        </div>
      )}
    </>
  );

  const MultiValue = (ps: any) => {
    if(props.value.length === 0) {
      return (<></>);
    }

    const {data} = ps;
    if(props.value.length === 1) {
      return <div className={"div-multi-value"}>{data.label}</div>;
    }

    let position = -1;
    props.value.forEach((v: any, i: number) => {
      if(v.value === data.value) {
        position = i;
      }
    });

    if(position !== 0) {
      return <div className={"div-multi-value"}></div>;
    }

    if(props.value.length === props.options.length) {
      return <div className={"div-multi-value"}>{t("allItemsAreSelected")} ({props.value.length})</div>;
    }

    return <div className={"div-multi-value"}>{props.value.length} {t("selected")}</div>;
}

  const customFilterOption = ({ value, label }: Option, input: string) =>
    (value !== "*" && isTextMatch(label, input)) ||
    (value === "*" && filteredOptions?.length > 0);

    const onInputChange = (
      inputValue: string,
      event: { action: InputAction }
    ) => {
      if (event.action === "input-change") setSelectInput(inputValue);
      else if (event.action === "menu-close" && selectInput !== "")
        setSelectInput("");
    };

    const onKeyDown = (e: React.KeyboardEvent<HTMLElement>) => {
      if ((e.key === " " || e.key === "Enter") && !selectInput)
        e.preventDefault();
    };

    const handleChange = (selected: any) => {
      if (
        selected.length > 0 &&
        !isAllSelected.current &&
        (selected[selected.length - 1].value === allOption.value || areObjectsEqual(filteredOptions, selected.sort(comparator)))
      )
        return props.onChange(
          [
            ...(props.value ?? []),
            ...props.options.filter(
              ({ label }: Option) => isTextMatch(label, selectInput) &&
                (props.value ?? []).filter((opt: Option) => opt.label === label).length === 0
            ),
          ].sort(comparator)
        );
      else if (
        selected.length > 0 &&
        selected[selected.length - 1].value !== allOption.value && !areObjectsEqual(selected.sort(comparator), filteredOptions)
      )
        return props.onChange(selected);
      else
        return props.onChange([
          ...props.value?.filter(({ label }: Option) => !isTextMatch(label, selectInput)),
        ]);
    };

    const customStyles = {
      control: (def: any) => ({
        ...def,
        ...props.styles.control
      }),
      multiValueLabel: (def: any) => ({
        ...def,
        backgroundColor: "lightgray",
      }),
      multiValueRemove: (def: any) => ({
        ...def,
        backgroundColor: "lightgray",
      }),
      valueContainer: (base: any) => ({
        ...base,
      }),
      option: (styles: any, { isSelected, isFocused }: any) => {
        return {
          ...styles,
          backgroundColor:
            isSelected && !isFocused
              ? null
              : isFocused && !isSelected
              ? styles.backgroundColor
              : isFocused && isSelected
              ? "#DEEBFF"
              : null,
          color: isSelected ? null : null,
        };
      },
      menu: (def: any) => ({ ...def, zIndex: 9999 }),
    };

    if (props.isSelectAll && props.options.length !== 0) {
      isAllSelected.current = areObjectsEqual(filteredSelectedOptions, filteredOptions);
  
      if (filteredSelectedOptions?.length > 0) {
        if (filteredSelectedOptions?.length === filteredOptions?.length)
          selectAllLabel.current = t("allItemsAreSelected") + ` (${filteredOptions.length})`;
        else
          selectAllLabel.current = `${filteredSelectedOptions?.length} / ${filteredOptions.length} ` + t("selected");
      } else {
        selectAllLabel.current = t("selectAll");
      }
  
      allOption.label = selectAllLabel.current;
  
      return (
        <ReactSelect
          {...props}
          inputValue={selectInput}
          onInputChange={onInputChange}
          onKeyDown={onKeyDown}
          options={[allOption, ...props.options]}
          onChange={handleChange}
          components={{
            Option: Option,
            Input: Input,
            MultiValue: MultiValue,
            ...props.components,
          }}
          filterOption={customFilterOption}
          menuPlacement={props.menuPlacement ?? "auto"}
          styles={customStyles}
          isMulti
          closeMenuOnSelect={false}
          tabSelectsValue={false}
          backspaceRemovesValue={false}
          hideSelectedOptions={false}
          blurInputOnSelect={false}
          placeholder={t("search")}
          noOptionsMessage={() => t("noOptions")}
        />
      );
    }

    return (
      <div className={styles["multiselect"]}>
        <ReactSelect
          {...props}
          inputValue={selectInput}
          onInputChange={onInputChange}
          filterOption={customFilterOption}
          components={{
            Input: Input,
            MultiValue: MultiValue,
            ...props.components,
          }}
          menuPlacement={props.menuPlacement ?? "auto"}
          onKeyDown={onKeyDown}
          tabSelectsValue={false}
          hideSelectedOptions={true}
          backspaceRemovesValue={false}
          blurInputOnSelect={true}
          placeholder={t("search")}
          noOptionsMessage={() => t("noOptions")}
        />
      </div>
    );
  };

export default MultiSelect;