import styles from "./AutoRefresh.module.scss";
import React, { useState, useEffect } from "react";
import useTranslation from "next-translate/useTranslation";
import { RefreshIcon } from "@components/Icons";

const AutoRefresh = ({refreshAction, globalState, setGlobalState, autoRefeshTimeout, setAutoRefeshTimeout}: any) => {
  const durations = [30, 60, 90, 0];
  const defaultDuration = 60;

  const [selectedDuration, setSelectedDuration] = useState(defaultDuration);
  const [countDownValue, setCountDownValue] = useState(defaultDuration);
  const [isInit, setIsInit] = useState(false);
  const [isCallingRefresh, setIsCallingRefresh] = useState(false);
  const { t } = useTranslation("common");

  function setDuration(d: any) {
    setSelectedDuration(d);
    startCountDown(d);
  }

  function callRefresh(sd: any) {
    if(isCallingRefresh) {
      return;
    }

    setIsCallingRefresh(true);
    
    if(autoRefeshTimeout) {
      clearTimeout(autoRefeshTimeout);
      setAutoRefeshTimeout(0);
    }


    refreshAction(() => {  
      startCountDown(sd); 
      setIsCallingRefresh(false);
    });
  }

  function startCountDown(d: any) {
    if(autoRefeshTimeout) {
      clearTimeout(autoRefeshTimeout);
      setAutoRefeshTimeout(0);
    }
    
    if(d == 0) {
      return;
    }

    setCountDownValue(d);
    const t = setTimeout(() => { countDown(d, d); }, 1000);
    setAutoRefeshTimeout(t);
  }

  function countDown(d: any, sd: any) {
    const newTime = d - 1;
    setCountDownValue(newTime);

    if (newTime === 0) {
      clearTimeout(autoRefeshTimeout);
      setAutoRefeshTimeout(0);
      callRefresh(sd);
      return;
    }

    const t = setTimeout(() => { countDown(newTime, sd); }, 1000);
    setAutoRefeshTimeout(t);
  }

	useEffect(() => { 
    if(!isInit) {
      setIsInit(true);
      startCountDown(defaultDuration); 
    }
  });

  return (
    <div className={styles["refresh-countdown"]}>
      <div className={styles["refresh-icon"]}
        onClick={() => { callRefresh(selectedDuration); }}><RefreshIcon height={28} width={28} /></div>
      <div className={styles["countdown"]}>
        <div className={styles["countdown-value"]} 
          onClick={(e) => { 
            setGlobalState({...globalState, showDurations: !globalState.showDurations}); 
            e.stopPropagation();
            return false;
          }}>
          {selectedDuration == 0 ? t("noRefresh") : countDownValue}
        </div>
        <div className={`${styles["countdown-dropdown"]} ${globalState.showDurations ? '' : 'hide'}`}>
          {durations.map((d: any, i: number) => {
            return (
              <div className={styles["countdown-refresh-option"]}
                key={"auto-refresh-option-" + d}
                onClick={() => {setDuration(d);}}>{d == 0 ? t("noRefresh") : d}</div>
            );
          })}
        </div>
      </div>
    </div>
  );
};

export default AutoRefresh;