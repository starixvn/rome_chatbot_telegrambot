import ReactLoading from "react-loading";
import styles from "./BlockUI.module.scss";

const BlockUI = () => {
  return (
    <div className={styles["block-ui"]}>
      <ReactLoading type="spinningBubbles" color="#fff" />
    </div>
  );
};

export default BlockUI;
