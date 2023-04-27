import useTranslation from "next-translate/useTranslation";
import { useRouter } from "next/router";

export default function LockPage() {
  const router = useRouter();
  const { t } = useTranslation("common");
  const { isActive, isLocked } = router.query;

  const message = isActive === "false" ? t("AccountClosed") : isLocked === "true" ? t("AccountClocked") : t("AccountProtected");

  return <p>{message}</p>;
}
