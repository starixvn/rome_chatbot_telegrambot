/* eslint-disable react-hooks/exhaustive-deps */
import { useEffect, useState } from "react";
import cn from "classnames";
import { fetchWrapper } from "src/lib/fetch-wrapper";
import { characters } from "src/constants/characters";
import { CheckMarkIcon, ErrorIcon } from "@components/Icons";
import { SingleSelect } from "@components/Form/Select";
import styles from "./NewAccount.module.scss";

type Props = {
  actorName: string;
  onChange: (newAccountName: string) => void;
};

export const NewAccountName = ({ actorName, onChange }: Props) => {
  const [suggestName, setSuggestName] = useState("");
  const [isCorrectName, setIsCorrectName] = useState(true);

  useEffect(() => {
    handleGetSuggestName(actorName);
  }, []);

  useEffect(() => {
    if (suggestName !== "") {
      handleCheckExistName();
    }
  }, [suggestName]);

  const handleGetSuggestName = async (actorName: string) => {
    const data = await fetchWrapper.get(`/api/customer/suggest/${actorName}`);
    setSuggestName(data.suggestName);
    handleOnChange(data.suggestName);
  };

  const handleCheckExistName = async () => {
    const username = actorName.concat(suggestName);
    const data = await fetchWrapper.get(`/api/customer/exist/${username}`);
    if (!data.isExist) {
      setIsCorrectName(true);
    } else {
      onChange("");
      setIsCorrectName(false);
    }
  };

  const handleSelectChange = (e: React.ChangeEvent<HTMLSelectElement>, index: number) => {
    const result = suggestName.split("");
    result[index] = e.target.value;
    setSuggestName(result.join(""));
    handleOnChange(result.join(""));
  };

  const handleOnChange = (newSuggestName: string) => {
    onChange(actorName.concat(newSuggestName));
  };

  const renderSelect = (suggestName: string) => {
    return Array.from(suggestName).map((item, index) => {
      return (
        <div className={styles.item} key={`${item}-${index}`}>
          <SingleSelect handleChange={(e) => handleSelectChange(e, index)} options={characters} selectedValue={item.toString()} />
        </div>
      );
    });
  };

  return (
    <div className={cn(styles.account, { [styles.error]: !isCorrectName })}>
      <div className={styles.name}>{actorName}</div>
      {suggestName ? renderSelect(suggestName) : ""}
      <div className={styles.icon}>{isCorrectName ? <CheckMarkIcon height={16} width={16} /> : <ErrorIcon height={16} width={16} />}</div>
    </div>
  );
};
