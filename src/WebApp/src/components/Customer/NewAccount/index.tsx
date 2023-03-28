/* eslint-disable react-hooks/exhaustive-deps */
import { useEffect, useState } from "react";
import { characters } from "src/constants/characters";
import { CheckMarkIcon, ErrorIcon } from "@components/Icons";
import { SingleSelect } from "@components/Form/Select";
import styles from "./NewAccount.module.scss";

export const getSuggestAccountName = async (actorName: string) => {
  const url = `/api/customer/suggest/${actorName}`;
  const res = await fetch(url);
  return await res.json();
};

export const checkExistAccountName = async (username: string) => {
  const url = `/api/customer/exist/${username}`;
  const res = await fetch(url);
  return await res.json();
};

type Props = {
  actorName: string;
  onChange: (newAccountName: string) => void;
};

export const NewAccountName = ({ actorName, onChange }: Props) => {
  const [suggestName, setSuggestName] = useState("");
  const [isCorrectName, setIsCorrectName] = useState(false);

  useEffect(() => {
    handleGetSuggestName(actorName);
  }, []);

  useEffect(() => {
    if (suggestName !== "") {
      handleCheckExistName();
    }
  }, [suggestName]);

  const handleGetSuggestName = async (actorName: string) => {
    const data = await getSuggestAccountName(actorName);
    setSuggestName(data.suggestName);
    handleOnChange(data.suggestName);
  };

  const handleCheckExistName = async () => {
    const username = actorName.concat(suggestName);
    const data = await checkExistAccountName(username);
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
    <div className={styles.account}>
      <div className={styles.name}>{actorName}</div>
      {suggestName ? renderSelect(suggestName) : ""}
      <div className={styles.icon}>{isCorrectName ? <CheckMarkIcon height={16} width={16} /> : <ErrorIcon height={16} width={16} />}</div>
    </div>
  );
};
