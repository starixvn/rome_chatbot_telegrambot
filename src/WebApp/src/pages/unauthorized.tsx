import Image from "next/image";
import styles from "../styles/UnAuthorized.module.scss";
import AccessDeniedImg from "../../public/assets/access_denied.png";
import useTranslation from "next-translate/useTranslation";

export default function Unauthorized() {
  const { t } = useTranslation("common");
  return (
    <div className={styles.Unauthorized}>
      <Image src={AccessDeniedImg} alt="Access Denied"></Image>
      <p className={styles.title}>{t("AccessDenied")}</p>
      <p>{t("UnauthorizedMessage")}</p>
    </div>
  );
}
