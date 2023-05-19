import { useRouter } from "next/router";
import useTranslation from "next-translate/useTranslation";
import styles from "../styles/Kickout.module.scss";

export default function LockPage() {
  const router = useRouter();
  const { t } = useTranslation("common");
  const { key } = router.query;
  return (
    <div className={styles.container}>
      <p>{t(key as string)}</p>
    </div>
  );
}
