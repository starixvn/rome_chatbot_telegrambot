import styles from "./EditAutoTransferPage.module.scss";
import React from "react";
import useTranslation from "next-translate/useTranslation";
import { CalendarIcon } from "@components/Icons";

type CustomerListDataViewModel = {
    customers: any;
    totalPages: any;
    expandStatus: any;
    setExpandStatus: any;
    isExpandAll: any;
    setIsExpandAll: any;
    editStatus: any;
    editSchedule: any;
    selectedCustomers: any;
    setSelectedCustomers: any;
};

const CustomerListComponent = ({ 
    customers,
    expandStatus,
    setExpandStatus,
    isExpandAll,
    setIsExpandAll,
    editStatus,
    editSchedule,
    selectedCustomers,
    setSelectedCustomers,
    ...props
}: CustomerListDataViewModel) => {
    const { t } = useTranslation("editautotransfer");

    function setSelectedAllCustomers() {
        const checkAll = isSelectedAll();
        const selecteds = { ...selectedCustomers };
        for(let i=0; i<customers.length; i++) {
            selecteds[i] = !checkAll;
        }

        setSelectedCustomers(selecteds);
    }

    function isSelectedAll() {
        const selecteds = customers.filter((c: any,i: number) => selectedCustomers[i]);
        return selecteds.length === customers.length;
    }

    function setSelectedCustomer(_: any, i: number) {
        const selecteds = { ...selectedCustomers };
        selecteds[i] = !selecteds[i];
        setSelectedCustomers(selecteds);
    }

    return (
        <div className={styles["content"]}>
            <table className={"tbl-report"}>
                <thead>
                    <tr>
                        <th>
                            { 
                                customers.length > 0 && <input type="checkbox" 
                                    checked={isSelectedAll()}
                                    onClick={() => { setSelectedAllCustomers(); }}/>
                            }
                        </th>
                        <th>{t("username")}</th>
                        <th>{t("autoTransfer")}</th>
                        <th>
                            <div className={"div-center"}>
                                <div className={isExpandAll ? 'icon-collapse' : 'icon-expand'} onClick={() => {
                                    const status = { ...expandStatus };
                                    for(let i=0; i<customers.length; i++) {
                                        status[i] = !isExpandAll;
                                    }

                                    setExpandStatus(status);
                                    setIsExpandAll(!isExpandAll);
                                }}>
                                </div>
                            </div>
                        </th>
                    </tr>
                </thead>
                <tbody>
                    {customers.length === 0 && <tr><td colSpan={4} className="text-center">{t("noInformationIsAvailable")}</td></tr>}

                    {customers.length > 0 && customers.map((customer: any, i: number) => {
                        return (
                            <>
                                <tr key={i} className={"row-status-" + customer.status}>
                                    <td className={"text-center"}>
                                        <input type="checkbox" 
                                            checked={selectedCustomers[i]}
                                            onClick={() => { setSelectedCustomer(customer, i); }}/>
                                    </td>
                                    <td className={"text-center"}>{customer.username}</td>
                                    <td className={"text-center"}>
                                        <div className={styles["box-auto-calendar"]}>
                                            <div className={styles[customer.enableAutoTransfer ? "div-auto-transfer-on" : "div-auto-transfer-off"]}
                                                onClick={() => { editStatus(customer); }}>
                                                <div className={styles["toggle"]}></div>
                                            </div>
                                            {
                                                customer.enableAutoTransfer && <div className={styles["icon-calendar"]}
                                                        onClick={() => { editSchedule(customer); }}>
                                                        <CalendarIcon height={16} width={16} />
                                                    </div>
                                            }
                                        </div>
                                    </td>
                                    <td className={"text-center"}>
                                        {
                                            customer.enableAutoTransfer && <div className={"div-center"}>
                                                    <div className={expandStatus[i] ? 'icon-collapse' : 'icon-expand'} onClick={() => {
                                                        const status = { ...expandStatus };
                                                        status[i] = !status[i];
                                                        setExpandStatus(status);
                                                    }}>
                                                    </div>
                                                </div>
                                        }
                                    </td>
                                </tr>
                                {
                                    customer.enableAutoTransfer && <>
                                        <tr className={"row-detail " + (expandStatus[i] ? '' : 'hide')}>
                                            <td colSpan={4} className={"text-left"}>
                                                {t("scheduleInfo")}<span className={"txt-bold"}>{customer.autoTransferDates.map((d: any) => t("day" + d)).join(' - ')}</span>
                                            </td>
                                        </tr>
                                    </>
                                }
                            </>
                            )
                    })}
                </tbody>
            </table>
        </div>
    );
};

export default CustomerListComponent;