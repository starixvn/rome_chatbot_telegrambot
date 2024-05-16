import React, { useState } from "react";
import { Agent, Master, Super } from "src/constants/role-id";
import styles from "./Layout.module.scss";
import AutoRefresh from "../AutoRefresh";

type Props = {
  children: React.ReactNode;
  title: string;
  roleId: number;
  isSpecialSite: boolean;
  titleIcon?: any;
  showAutoRefresh?: boolean;
  refreshAction?: any;
};

const classNameByRoleMapper: { [roleId: number]: string } = {
  [Agent]: "agent",
  [Master]: "master",
  [Super]: "super",
};

const Layout: React.FC<Props> = ({ children, title, roleId, isSpecialSite, titleIcon, showAutoRefresh, refreshAction }) => {
  const className = classNameByRoleMapper[roleId];
  const [autoRefeshState, setAutoRefeshState] = useState<any>({});
  const [autoRefeshTimeout, setAutoRefeshTimeout] = useState<any>(0);

  return (
    <div className={`${styles.container} ${isSpecialSite ? styles["special-site"] : ""}`}
      onClick={() => { 
        setAutoRefeshState({...autoRefeshState, showDurations: false});
      }}>      
      <div className={`${styles.header} ${styles[className]}`}>
        {titleIcon}{title}{showAutoRefresh && <AutoRefresh refreshAction={refreshAction} 
          globalState={autoRefeshState} setGlobalState={setAutoRefeshState} 
          autoRefeshTimeout={autoRefeshTimeout} setAutoRefeshTimeout={setAutoRefeshTimeout} />}
      </div>
      <div className={styles.main}>{children}</div>
    </div>
  );
};

export default Layout;
