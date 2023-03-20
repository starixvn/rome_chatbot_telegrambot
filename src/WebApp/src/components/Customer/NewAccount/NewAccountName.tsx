/* eslint-disable react-hooks/exhaustive-deps */
import { useEffect, useState } from "react";
import { characters } from "src/constants/characters";
import { CheckMarkIcon } from "@components/Icons/Icons";
import { SingleSelect } from "@components/Form/Select/SingleSelect";

export const getSuggestAccountName = async (actorName: string) => {
  const url = `/api/customer/suggest/${actorName}`;
  const res = await fetch(url);
  return await res.json();
}

export const checkExistAccountName = async (username: string) => {
  const url = `/api/customer/exist/${username}`;
  const res = await fetch(url);
  return await res.json();
}

type Props = {
  actorName: string,
  onChange: (newAccountName: string) => void
}

export const NewAccountName = ({ actorName, onChange }: Props) =>
{
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
        <SingleSelect
          key={`${item}-${index}`}
          handleChange={(e) => handleSelectChange(e, index)}
          options={characters}
          selectedValue={item.toString()}
        />
      );
    });
  };

  return (
    <>
      <span>{actorName}</span>
      {suggestName ? renderSelect(suggestName) : ""}
      {isCorrectName ? <CheckMarkIcon height={16} width={16} /> : null}
    </>
  );
};
