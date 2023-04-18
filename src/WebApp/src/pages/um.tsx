import { useRouter } from "next/router";
import UM from "@features/CommonPage/Um";

export default function UMPage() {
  const router = useRouter();
  const umInfo = new URLSearchParams(router.asPath.replace("/um", ""));
  return <UM from={umInfo.get("from") || "0:00"} to={umInfo.get("to") || "0:00"} />;
}
