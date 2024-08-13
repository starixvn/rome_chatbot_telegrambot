import useTranslation from 'next-translate/useTranslation';
import React, { useState } from 'react';
import { useTracking } from 'src/hooks/tracking-activity';
import { User } from 'src/lib/session';

import { ArrowDownIcon, ArrowRightIcon } from '@components/Icons';

import styles from './TotalBetsDetails.module.scss';

type BasePageViewModel = {
  user: User;
  totalBetsDetailsData: any;
  collapseBetTypeStatus: any;
  setCollapseBetTypeStatus: any;
  isSpecialSite: any;
};


export const TotalBetsDetails = ({
  user, totalBetsDetailsData, collapseBetTypeStatus, setCollapseBetTypeStatus, isSpecialSite, ...props }: BasePageViewModel) => {
  const { t } = useTranslation("totalbets");
  useTracking("/totalbets");

  const betTypeIds = {
      ft1x2: 5,
      _1h1x2: 15,
      ou: 3,
      _1hOU: 8,
      others: 0
  };
  
  const hdpTeams = {
      home: 1,
      away: 2
  };

  function numberWithCommas(x: any) {
    const v = Math.round(x * 100) / 100;
    return v.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
  }

  function formatTurnover(value: any) {
    if(value >= 1000000) {
      return numberWithCommas(value / 1000000) + "M";
    }

    if(value >= 1000) {
      return numberWithCommas(value / 1000) + "K";
    }

    return numberWithCommas(Math.round(value));
  }

  function toggleBetType(id: any) {
    const newValue = {...collapseBetTypeStatus};
    newValue[id] = !newValue[id];
    setCollapseBetTypeStatus(newValue);
  }

  function isCollapseBetType(id: any) {
    return collapseBetTypeStatus[id];
  }

  const match = totalBetsDetailsData?.match;

  return (
    <>
      {!totalBetsDetailsData && <label>{t("loading")}</label>}
      {(totalBetsDetailsData && !match) && <label>{t("detailsNotAvailable")}</label>}
      {(totalBetsDetailsData && match) && <>
        <div className={styles["top-detail-total-bets"]}>
          <div className={styles["league-name"]}>{match.leagueName}</div>
          <div>
            <span className={styles["name"]}><span className={styles["home-name"]}>{match.homeName}{match.isNeutral ? ' (N)' : ''}</span>
            {match.liveIndicator == 2 ? <span className={styles["score"]}>{match.homeScore + " - " + match.awayScore} </span> : <span className={styles["vs"]}> -vs- </span>}
            <span className={styles["away-name"]}>{match.awayName}</span></span>
            {match.liveIndicator == 2 && <span className={styles["badge-live-full"]}> {t('live')}</span>}
            {match.liveIndicator == 1 && <> <span className={styles["badge-live"]}> {t('live')}</span><span className={styles["showtime"]}>[{match.globalShowTime_Time}]</span></>}
            {match.liveIndicator == 0 && <span className={styles["showtime"]}> [{match.globalShowTime_Date} {match.globalShowTime_Time}]</span>}
          </div>
          <div className={styles["turnover"]}>{isSpecialSite ? t("memberTurnover") : t("turnover")}: <span>{match.formattedTurnover}</span></div>
        </div>
          {totalBetsDetailsData.betTypes.map((b: any, i: number) => {
            let to;
             switch(b.betTypeId) {
              case betTypeIds.others:
                return (
                  <div className={styles["block-detail"]}>
                    <div className={styles["bet-type-header"]}
                      key={"bet-type-header-" + b.betTypeId}>
                      {t("others")} - {b.formattedTurnover}
                      <div className={styles["right-icon"]} onClick={() => { toggleBetType(b.betTypeId); }}>
                        {isCollapseBetType(b.betTypeId) ? <ArrowRightIcon height={20} width={20} /> : <ArrowDownIcon height={20} width={20} />}
                      </div>
                    </div>
                    <div className={`${styles["list-item"]} ${isCollapseBetType(b.betTypeId) ? 'hide' : ''}`}>
                      <div className={styles["big-item"]}>
                        <div>{t("others")}</div>
                        <div><strong>{formatTurnover(b.totalTO)}</strong></div>
                      </div>
                    </div>
                  </div>
                );
              case betTypeIds.ft1x2:
              case betTypeIds._1h1x2:
                to = totalBetsDetailsData.toBetTypes.filter((t: any) => t.betTypeId == b.betTypeId)[0];
                return (
                  <div className={styles["block-detail"]}>
                    <div className={styles["bet-type-header"]}
                      key={"bet-type-header-" + b.betTypeId}>
                      {b.betTypeName} - {b.formattedTurnover}
                      <div className={styles["right-icon"]}  onClick={() => { toggleBetType(b.betTypeId); }}>
                        {isCollapseBetType(b.betTypeId) ? <ArrowRightIcon height={20} width={20} /> : <ArrowDownIcon height={20} width={20} />}
                      </div>
                    </div>
                    <div className={`${styles["list-item"]} ${isCollapseBetType(b.betTypeId) ? 'hide' : ''}`}>
                      <div className={styles["width-auto"]}>
                        <div className={styles["item"]}>
                          <div>{t("home")}</div>
                          <div><strong>{formatTurnover(to.homeTO)}</strong></div>
                        </div>
                        <div className={styles["item"]}>
                          <div>{t("draw")}</div>
                          <div><strong>{formatTurnover(to.drawTO)}</strong></div>
                        </div>
                        <div className={styles["item"]}>
                          <div>{t("away")}</div>
                          <div><strong>{formatTurnover(to.awayTO)}</strong></div>
                        </div>
                      </div>
                    </div>
                  </div>
                );
              case betTypeIds.ou:
              case betTypeIds._1hOU:
                to = totalBetsDetailsData.toBetTypes.filter((t: any) => t.betTypeId == b.betTypeId);
                let overTotal = 0;
                let underTotal = 0;
                for(let i=0; i<to.length; i++) {
                  const t = to[i];
                  overTotal += t.overTO;
                  underTotal += t.underTO;
                }
                
                return (
                  <div className={styles["block-detail"]}>
                    <div className={styles["bet-type-header"]}
                      key={"bet-type-header-" + b.betTypeId}>
                      {b.betTypeName} - {b.formattedTurnover}
                      <div className={styles["right-icon"]}  onClick={() => { toggleBetType(b.betTypeId); }}>
                        {isCollapseBetType(b.betTypeId) ? <ArrowRightIcon height={20} width={20} /> : <ArrowDownIcon height={20} width={20} />}
                      </div>
                    </div>
                    <div className={`${styles["list-item"]} ${isCollapseBetType(b.betTypeId) ? 'hide' : ''}`}>
                      <div className={styles["big-item"]}>
                        <div>{t("over")}</div>
                        <div><strong>{formatTurnover(overTotal)}</strong></div>
                      </div>
                      <div className={styles["value"]}>
                        {to.map((t: any, i: number) => {
                          return (
                            <>
                            {t.overTO > 0 && <div className={styles["item"]}>
                              <div>{parseFloat(t.hdp)}</div>
                              <div><strong>{formatTurnover(t.overTO)}</strong></div>
                            </div>}
                            </>
                          );
                        })}
                      </div>
                    </div>
                    <div className={`${styles["list-item"]} ${isCollapseBetType(b.betTypeId) ? 'hide' : ''}`}>
                      <div className={styles["big-item"]}>
                        <div>{t("under")}</div>
                        <div><strong>{formatTurnover(underTotal)}</strong></div>
                      </div>
                      <div className={styles["value"]}>
                        {to.map((t: any, i: number) => {
                          return (
                            <>
                            {t.underTO > 0 && <div className={styles["item"]}>
                              <div>{parseFloat(t.hdp)}</div>
                              <div><strong>{formatTurnover(t.underTO)}</strong></div>
                            </div>}
                            </>
                          );
                        })}
                      </div>
                    </div>
                  </div>
                );
              default:
                to = totalBetsDetailsData.toBetTypes.filter((t: any) => t.betTypeId == b.betTypeId);
                let homeTotal = 0;
                let awayTotal = 0;
                for(let i=0; i<to.length; i++) {
                  const t = to[i];
                  homeTotal += t.homeTO;
                  awayTotal += t.awayTO;
                }

                return (
                  <div className={styles["block-detail"]}>
                    <div className={styles["bet-type-header"]}
                      key={"bet-type-header-" + b.betTypeId}>
                      {b.betTypeName} - {b.formattedTurnover}
                      <div className={styles["right-icon"]}  onClick={() => { toggleBetType(b.betTypeId); }}>
                        {isCollapseBetType(b.betTypeId) ? <ArrowRightIcon height={20} width={20} /> : <ArrowDownIcon height={20} width={20} />}
                      </div>
                    </div>
                    <div className={`${styles["list-item"]} ${isCollapseBetType(b.betTypeId) ? 'hide' : ''}`}>
                      <div className={styles["big-item"]}>
                        <div>{t("home")}</div>
                        <div><strong>{formatTurnover(homeTotal)}</strong></div>
                      </div>
                      <div className={styles["value"]}>
                        {to.map((t: any, i: number) => {
                          return (
                            <>
                            {t.homeTO > 0 && <div className={styles["item"]}>
                              <div>{parseFloat(t.hdp) == 0 ? '' : t.teamHDP==hdpTeams.away ? '+' : '-'}{parseFloat(t.hdp)}</div>
                              <div><strong>{formatTurnover(t.homeTO)}</strong></div>
                            </div>}
                            </>
                          );
                        })}
                      </div>
                    </div>
                    <div className={`${styles["list-item"]} ${isCollapseBetType(b.betTypeId) ? 'hide' : ''}`}>
                      <div className={styles["big-item"]}>
                        <div>{t("away")}</div>
                        <div><strong>{formatTurnover(awayTotal)}</strong></div>
                      </div>
                      <div className={styles["value"]}>
                        {to.map((t: any, i: number) => {
                          return (
                            <>
                            {t.awayTO > 0 && <div className={styles["item"]}>
                              <div>{parseFloat(t.hdp) == 0 ? '' : t.teamHDP==hdpTeams.away ? '-' : '+'}{parseFloat(t.hdp)}</div>
                              <div><strong>{formatTurnover(t.awayTO)}</strong></div>
                            </div>}
                            </>
                          );
                        })}
                      </div>
                    </div>
                  </div>
                );
            }               
        })}
        </>
      }
      <div className={styles["text-align-right"]}>
        <button className="btn-submit width-auto" type="button"
            onClick={() => {
              location.href= "../totalbets"; 
            }}>
            {t("back")}
        </button>
      </div>  
    </>
  );
};