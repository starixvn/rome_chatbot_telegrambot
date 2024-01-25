import styles from "./AutoTransferMonitorPage.module.scss";
import React from "react";
import useTranslation from "next-translate/useTranslation";

type MonitorDataViewModel = {
    transfers: any;
    totalPending: number;
    totalProcessing: number;
    totalFailed: number;
    sortedBy: any;
    sortedDirection: any;
    setSortedBy: any;
    setSortedDirection: any;
    setSortValue: any;
    expandStatus: any;
    setExpandStatus: any;
    isExpandAll: any;
    setIsExpandAll: any;
};
 
const MonitorComponent = ({ 
    sortedBy, 
    sortedDirection, 
    setSortedBy, 
    setSortedDirection, 
    setSortValue, 
    expandStatus,
    setExpandStatus,
    isExpandAll,
    setIsExpandAll,
    ...props }: MonitorDataViewModel) => {

    const sortedByValues = {
        username: 1,
        transferAmount: 3,
        status: 2
    };

    const sortedDirectionValues = {
        asc: 0,
        desc: 1
    };

    const { t } = useTranslation("viewautotransfermonitor");

    function updateSort(val: any) {
        const direction =  sortedDirection === sortedDirectionValues.asc ? sortedDirectionValues.desc : sortedDirectionValues.asc ;
        const newDirection = sortedBy === val ? direction : sortedDirectionValues.asc;
        setSortedDirection(newDirection);
        setSortedBy(val); 
        setExpandStatus({});
        setSortValue(val * 10 + newDirection);
    } 

    return (
        <div className={styles["content"]}>
            <div className={styles["div-status-all"]}>
                <div className={styles["div-pending-all"]}>
                    <span className={styles["text"]}>{t("pending")}</span>
                    <span className={styles["spn-pending-count"]}>{props.totalPending.toString()}</span>
                </div>
                <div className={styles["div-processing-all"]}>
                    <span className={styles["text"]}>{t("processing")}</span>
                    <span className={styles["spn-processing-count"]}>{props.totalProcessing}</span>
                </div>
                <div className={styles["div-failed-all"]}>
                    <span className={styles["text"]}>{t("failed")}</span>
                    <span className={styles["spn-failed-count"]}>{props.totalFailed}</span>
                </div>
            </div>

            <table className={"tbl-report"}>
                <thead>
                    <tr>
                        <th>#</th>
                        <th className={"sorting " + (sortedBy === sortedByValues.username ? (sortedDirection === sortedDirectionValues.asc ? "sorting_asc" : "sorting_desc") : "")}
                            onClick={() => { updateSort(sortedByValues.username); }} >
                            {t("username")}
                            <span className={"icon-sort"}>
                                <div className={"icon-asc"}>▲</div>
                                <div className={"icon-desc"}>▼</div>
                            </span>
                        </th>
                        <th className={"sorting " + (sortedBy === sortedByValues.transferAmount ? (sortedDirection === sortedDirectionValues.asc ? "sorting_asc" : "sorting_desc") : "")}
                            onClick={() => { updateSort(sortedByValues.transferAmount); }} >
                            {t("transferAmount")}
                            <span className={"icon-sort"}>
                                <div className={"icon-asc"}>▲</div>
                                <div className={"icon-desc"}>▼</div>
                            </span>
                        </th>
                        <th className={"sorting " + (sortedBy === sortedByValues.status ? (sortedDirection === sortedDirectionValues.asc ? "sorting_asc" : "sorting_desc") : "")}
                            onClick={() => {  updateSort(sortedByValues.status); }} >
                            {t("status")}
                            <span className={"icon-sort"}>
                                <div className={"icon-asc"}>▲</div>
                                <div className={"icon-desc"}>▼</div>
                            </span>
                        </th>
                        <th>
                            <div className={"div-center"}>
                                <div className={isExpandAll ? 'icon-collapse' : 'icon-expand'} onClick={() => {
                                    const status = { ...expandStatus };
                                    for(let i=0; i<props.transfers.length; i++) {
                                        status[i] = !isExpandAll;
                                    }

                                    setIsExpandAll(!isExpandAll);
                                    setExpandStatus(status);
                                }}>
                                </div>
                            </div>
                        </th>
                    </tr>
                </thead>
                <tbody>
                    {props.transfers.length === 0 && <tr><td colSpan={7} className="text-center">{t("noInformationIsAvailable")}</td></tr>}
                    {props.transfers.length > 0 && props.transfers.map((transfer: any, i: number) => {
                        const amountClass = (transfer.transferAmount || 0) < 0 ? " format-negative" : "";
                        const status = transfer.status.toLowerCase() === 'waiting' ? 'processing' : transfer.status.toLowerCase();
                        
                        return (
                            <>
                                <tr key={i}>
                                    <td className={"text-center"}>{(i + 1).toString()}</td>
                                    <td className={"text-center"}>{transfer.username}</td>
                                    <td className={"text-center" + amountClass}>{transfer.transferAmountStr}</td>
                                    <td className={`text-center col-status ${status}`}>{t(status)}</td>
                                    <td className={"text-center"} align="center">
                                        <div className="div-center">
                                            <div className={expandStatus[i] ? 'icon-collapse' : 'icon-expand'} onClick={() => {
                                                const status = { ...expandStatus };
                                                status[i] = !status[i];
                                                setExpandStatus(status);
                                            }}>
                                            </div>
                                        </div>
                                    </td>
                                </tr>
                                <tr className={"row-detail " + (expandStatus[i] ? '' : 'hide')}>
                                    <td rowSpan={3}></td>
                                    <td className={"text-left"}>{t("firstLastName")}</td>
                                    <td colSpan={3} className={"text-center"}>{transfer.firstLastName}</td>
                                </tr>
                                <tr className={"row-detail " + (expandStatus[i] ? '' : 'hide')}>
                                    <td className={"text-left"}>{t("reason")}</td>
                                    <td colSpan={3} className={"text-center"}>{transfer.reason || ''}</td>
                                </tr>
                                <tr className={"row-detail " + (expandStatus[i] ? '' : 'hide')}>
                                    <td className={"text-left"}>{t("completedTime")}</td>
                                    <td colSpan={3} className={"text-center"}>{transfer.completedTime || ''}</td>
                                </tr>
                            </>
                            )
                        })}
                </tbody>
            </table>
        </div>
    );
}


export default MonitorComponent;