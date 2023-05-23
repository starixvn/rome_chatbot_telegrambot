import Image from "next/image";
import ErrorImg from "../../../public/assets/error.png";
import styles from "./Error.module.scss";
import { useRouter } from "next/router";

const errorMsg: { [locale: string]: string } = {
  ["en-US"]: "An error has occurred, please contact CSD for details.",
  ["zh-CN"]: "很抱歉，服务器发生意外错误，请联系我们的客服协助处理。",
  ["zh-TW"]: "很抱歉，伺服器發生意外錯誤，請聯繫我們的客服協助處理。",
  ["th-TH"]: "มีปัญหาที่ไม่คาดคิดเกิดขึ้น โปรดติดต่อฝ่ายสนับสนุนลูกค้าหากท่านต้องการความช่วยเหลือ",
  ["vi-VN"]: "Rất tiếc, đã xảy ra lỗi không mong muốn với máy chủ. Vui lòng liên hệ với bộ phận chăm sóc khách hàng của chúng tôi để được hỗ trợ.",
};

const ErrorComponent = ({ statusCode }: { statusCode: number }) => {
  const router = useRouter();
  return (
    <div className={styles.error}>
      <Image src={ErrorImg} alt="Error"></Image>
      <p>{`${errorMsg[router.locale as string]}(${statusCode})`} </p>
    </div>
  );
};

export default ErrorComponent;
