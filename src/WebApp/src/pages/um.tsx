import styles from "../styles/UM.module.scss";
import Image from "next/image";
import UmImg from "../../public/assets/maintain.png";
import { useRouter } from "next/router";

export default function UMPage() {
  const router = useRouter();
  const { isSpecialSite, from, to } = router.query;
  return (
    <div className={styles.um}>
      <Image src={UmImg} alt="Under Maintenance"></Image>
      <p className={styles.title}>Under Maintenance</p>
      <p>We are having regular server maintenance.</p>
      <p>{`The website is currently unavailable from ${from} until ${to} (GMT +8).`}</p>
      <p>Please come back later.</p>
      <hr />
      {isSpecialSite ? (
        <>
          <p>Hệ thống của chúng tôi đang trong thời gian bảo trì định kỳ.</p>
          <p>{`Trang mạng đang ngừng hoạt động từ ${from} cho đến ${to} (GMT +8).`}</p>
          <p>Vui lòng quay trở lại sau.</p>
        </>
      ) : (
        <>
          <p>{`本公司將於 ${from} 至 ${to} (GMT +8) 對服務器進行例行維護。`}</p>
          <p>期間所有的網頁將無法登入。</p>
          <p>請您稍候再來。</p>
        </>
      )}
    </div>
  );
}
