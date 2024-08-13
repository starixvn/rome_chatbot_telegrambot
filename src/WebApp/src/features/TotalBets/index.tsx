import useTranslation from 'next-translate/useTranslation';
import React from 'react';
import { useTracking } from 'src/hooks/tracking-activity';
import { User } from 'src/lib/session';

import styles from './TotalBets.module.scss';

type BasePageViewModel = {
  user: User;
  totalBetsData: any;
};

export const TotalBets = ({ user, totalBetsData, ...props }: BasePageViewModel) => {
  const { t } = useTranslation("totalbets");
  useTracking();

  function viewDetail(matchId: any) {
    const url = location.href.replace("/totalbets", "/totalbetsdetails/") + matchId;
    location.href = url;
  }

  return (
    <>
      {!totalBetsData && <label>{t("loading")}</label>}
      {totalBetsData && (
        <>
          <div className={styles["list-total-bets"]}>
            {totalBetsData.matches?.length === 0 && <div className="text-center">{t("noInformationIsAvailable")}</div>}
            {totalBetsData.matches?.length > 0 &&
              totalBetsData.matches.map((match: any, i: number) => {
                return (
                  <div className={styles["item-total-bets"]} key={"match-" + i + "-home"}>
                    {match.liveIndicator == 2 && (
                      <div className={styles["info"]}>
                        <div className={styles["score"]}>{match.homeScore + " - " + match.awayScore}</div>
                        <div className={styles["live"]}>
                          <span className={styles["badge-live-full"]}>{t("live")}</span>
                        </div>
                      </div>
                    )}
                    {match.liveIndicator == 1 && (
                      <div className={styles["info"]}>
                        <div className={styles["live"]}>
                          <span className={styles["badge-live"]}>{t("live")}</span>
                        </div>
                        <div className={styles["time"]} dangerouslySetInnerHTML={{ __html: match.globalShowTime_Time }}></div>
                      </div>
                    )}
                    {match.liveIndicator == 0 && (
                      <div className={styles["info"]}>
                        <div className={styles["time"]} dangerouslySetInnerHTML={{ __html: match.globalShowTime_Date }}></div>
                        <div className={styles["time"]} dangerouslySetInnerHTML={{ __html: match.globalShowTime_Time }}></div>
                      </div>
                    )}
                    <div className={styles["team-name"]}>
                      <div>
                        <div>
                          {match.homeName}
                          {match.isNeutral ? " (N)" : ""}
                        </div>
                        <div>{match.awayName}</div>
                      </div>
                    </div>
                    <div
                      className={styles["turnover"]}
                      onClick={() => {
                        viewDetail(match.matchId);
                      }}
                    >
                      <span className={styles["turnover-number"]}>{match.formattedTurnover}</span>
                    </div>
                  </div>
                );
              })}
          </div>
          {totalBetsData.matches?.length > 0 && (
            <div>
              <div className="txt-bold">{t("note")}:</div>
              <div>-{t("gmt8Note")}</div>
            </div>
          )}
        </>
      )}
    </>
  );
};
