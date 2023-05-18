import useTranslation from "next-translate/useTranslation";
import { useRouter } from "next/router";

export default function LockPage() {
  const router = useRouter();
  const { t } = useTranslation("common");
  const { key } = router.query;
  return <p>{t(key as string)}</p>;
}
