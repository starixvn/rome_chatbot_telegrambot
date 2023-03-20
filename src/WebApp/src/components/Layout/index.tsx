import React from "react";
import { Agent, Master, Super } from "src/constants/role-id";
import styles from "./Layout.module.scss";

type Props = {
  children: React.ReactNode;
  title: string;
  roleId: number;
};

const classNameByRoleMapper: { [roleId: number]: string } = {
  [Agent]: "agent",
  [Master]: "master",
  [Super]: "super",
};

const Layout: React.FC<Props> = ({ children, title, roleId }) => {
  const className = classNameByRoleMapper[roleId];
  return (
    <div className={styles.container}>
      <div className={`${styles.header} ${styles[className]}`}>{title}</div>
      <div className={styles.main}>{children}</div>
    </div>
  );
};

export default Layout;
