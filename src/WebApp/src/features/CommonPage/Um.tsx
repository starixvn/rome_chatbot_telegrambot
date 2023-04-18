import Image from "next/image";
import useTranslation from "next-translate/useTranslation";
import UmImg from "../../../public/assets/maintain.png";
import styles from "./Um.module.scss";

type Props = {
  from: string;
  to: string;
};

const UM = ({ from, to }: Props) => {
  const { t } = useTranslation("common");
  return (
    <div className={styles.um}>
      <Image src={UmImg} alt="Under Maintenance"></Image>
      <p className={styles.title}>{t("UnderMaintenance")}</p>
      <p>{t("UmMess1")}</p>
      <p>{t("UmMess2", { from, to })}</p>
      <p>{t("UmMess3")}</p>
      <hr />
      <p>{t("UmMessDefault1", { from, to })}</p>
      <p>{t("UmMessDefault2")}</p>
      <p>{t("UmMessDefault3")}</p>
    </div>
  );
};

export default UM;
