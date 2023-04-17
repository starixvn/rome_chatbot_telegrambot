import Image from "next/image";
import useTranslation from "next-translate/useTranslation";
import ErrorImg from "../../../public/assets/error.png";
import styles from "./Error.module.scss";

const ErrorComponent = () => {
  const { t } = useTranslation("common");
  return (
    <div className={styles.error}>
      <Image src={ErrorImg} alt="Error"></Image>
      <p>{t("ErrorPageMess")} </p>
    </div>
  );
};

export default ErrorComponent;
