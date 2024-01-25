import styles from "./EditAutoTransferPage.module.scss";
import React, { useState } from "react";
import { useForm } from "react-hook-form";
import useTranslation from "next-translate/useTranslation";
import BlockUI from "@components/BlockUI";
import { fetchWrapper } from "src/lib/fetch-client";
import { useRouter } from "next/router";
import Message from "@components/Message";

type EditMultipleViewModel = {
    onCustomers: any;
    offCustomers: any;
    uplineTransferOption: any;
    returnCustomerLists: any;
    transferrableDates: any;
};

type IFormInputs = {
    username: string;
};


const EditMultipleComponent = ({ 
    onCustomers,
    offCustomers,
    uplineTransferOption,
    returnCustomerLists,
    transferrableDates,
    ...props
}: EditMultipleViewModel) => {
    const { t } = useTranslation("editautotransfer");
    const methods = useForm<IFormInputs>({
      mode: "onSubmit",
      defaultValues: { username: '' },
      shouldFocusError: false,
    });
    const { formState, handleSubmit, control } = methods;
    const router = useRouter();
    const [updateResult, setUpdateResult] = useState<any>();
    const [selectedStatus_On, setSelectedStatus_On] = useState<boolean>(true);
    const [unselectedDates_On, setUnselectedDates_On] = useState<{ [key: number]: boolean}>({});
    const [selectedStatus_Off, setSelectedStatus_Off] = useState<boolean>(false);
    const [unselectedDates_Off, setUnselectedDates_Off] = useState<{ [key: number]: boolean}>({});

    const onNames = onCustomers.map((c: any) => c.username);
    const offNames = offCustomers.map((c: any) => c.username);

    const onSubmit = handleSubmit(async () => {
        const url = `/api/autotransfer/updateMultiple`;
        const allNames = onNames.concat(offNames).join(", ");

        const msg = t("confirmUpdateAutoTransfer") + " " + allNames;
        if (window.confirm(msg)) {
            const body = {
                onIds: onCustomers.map((c: any) => c.custId).join(","),
                offIds: offCustomers.map((c: any) => c.custId).join(","),
                onNames: onNames.join(","),
                offNames: offNames.join(","),
                onStatus: selectedStatus_On,
                offStatus: selectedStatus_Off,
                onSchedule: getSelectedSchedule(unselectedDates_On),
                offSchedule: getSelectedSchedule(unselectedDates_Off),
                oldTransferOption: uplineTransferOption
            };

            const data = await fetchWrapper.post<any>(url, body, router);
            setUpdateResult(data);
            if(data.code == 107 || data.code == 105 || (!data.hasError && data.hasWarning)) {
                setTimeout(() => { returnCustomerLists(true); }, 3000);
            }
        }
    });

    function getSelectedSchedule(unselectedDates: any) {
        const schedule = transferrableDates.filter((_: any, i: number) => !unselectedDates[i]);
        return schedule.join(',');
    }

    function isCurrectSelectAll(unselectedDates: any) {
        return !transferrableDates.some((_: any, i: number) => unselectedDates[i]);
    }

    function validateSchedule() {
        return (onCustomers.length === 0 || !selectedStatus_On || getSelectedSchedule(unselectedDates_On)) &&
            (offCustomers.length === 0 || !selectedStatus_Off || getSelectedSchedule(unselectedDates_Off));
    }

    function canEditMore() {
        return updateResult && updateResult.code != 108;
    }

    return (
        <>
            {formState.isSubmitting && <BlockUI />}
            {
                !updateResult  && <form className={styles["form-auto-transfer-settings"]} onSubmit={onSubmit}>
                    <div className={`${styles["div-on-customers"]} ${onCustomers.length ? '' : 'hide'}`}>
                        <div className={`${styles["frm-row"]} ${styles["h-auto"]} ${styles["frm-align-left"]}`}>                    
                            <div className={"txt-bold"}>{t("autoTransfer")}:</div>                            
                            <div className={styles["box-auto-calendar"]}>
                                <div className={styles["lbl-on-off"]} onClick={() => { setSelectedStatus_On(false); }}>{t("off")}</div>
                                <div className={styles[selectedStatus_On ? "div-auto-transfer-on" : "div-auto-transfer-off"]}
                                    onClick={() => { 
                                        const newVal = !selectedStatus_On; 
                                        setSelectedStatus_On(newVal);
                                        newVal && setUnselectedDates_On({});
                                    }}>
                                    <div className={styles["toggle"]}></div>
                                </div>
                                <div className={styles["lbl-on-off"]} onClick={() => { 
                                    setSelectedStatus_On(true);
                                    setUnselectedDates_On({});
                                    }}>{t("on")}</div>
                            </div>                          
                        </div>
                        <div className={`${styles["frm-row"]} ${styles["h-auto"]} ${styles["frm-align-left"]}`}>
                            <div className={"txt-bold"}>{t("schedule")}:</div>
                        </div>
                        <div className={`${styles["frm-row"]} ${styles["h-auto"]} ${styles["frm-align-left"]}`}>
                            <input type="checkbox" id="chk-all-on"
                                disabled={!selectedStatus_On}
                                checked={selectedStatus_On && isCurrectSelectAll(unselectedDates_On)} 
                                onClick={() => {
                                    const isCheckAll = isCurrectSelectAll(unselectedDates_On);
                                    const u = transferrableDates.map(() => isCheckAll);
                                    setUnselectedDates_On(u);
                                }}/>

                            <label htmlFor={"chk-all-on"} className={styles["lbl-for-ckb"]}>{t("selectAll")}</label>
                        </div>
                        <div className={`${styles["frm-row"]} ${styles["h-auto"]} ${styles["frm-align-left"]} ${styles["flex-wrap"]}`}>
                            {
                                transferrableDates.length > 0 && transferrableDates.map((d: any, i: number) => {
                                    return (
                                        <div className={`${styles["flex"]} ${styles["align-center"]}`} key={i}>
                                            <input type="checkbox" id={`chk-${i}-on`} 
                                                disabled={!selectedStatus_On}
                                                checked={selectedStatus_On && !unselectedDates_On[i]} 
                                                onClick={() => { 
                                                    const u = {...unselectedDates_On};
                                                    u[i] = !unselectedDates_On[i]; 
                                                    setUnselectedDates_On(u);
                                                }}/>
                                            <label htmlFor={`chk-${i}-on`} className={styles["lbl-for-ckb"]}>{t(`day${d}`)}</label>
                                        </div>
                                    );
                                })
                            }
                        </div>
                        <div className={`${styles["frm-row"]} ${styles["h-auto"]} ${styles["frm-align-left"]}`}>      
                            <div className={styles["div-usernames"]}>{onNames.join(", ")}</div>
                        </div>
                    </div>

                    <div className={`${styles["div-off-customers"]} ${offCustomers.length ? '' : 'hide'}`}>
                        <div className={`${styles["frm-row"]} ${styles["h-auto"]} ${styles["frm-align-left"]}`}>                      
                            <div className={"txt-bold"}>{t("autoTransfer")}:</div>                          
                            <div className={styles["box-auto-calendar"]}>
                                <div className={styles["lbl-on-off"]} onClick={() => { setSelectedStatus_Off(false); }}>{t("off")}</div>
                                <div className={styles[selectedStatus_Off ? "div-auto-transfer-on" : "div-auto-transfer-off"]}
                                    onClick={() => { 
                                        const newVal = !selectedStatus_Off; 
                                        setSelectedStatus_Off(newVal); 
                                        newVal && setUnselectedDates_Off({});
                                    }}>
                                    <div className={styles["toggle"]}></div>
                                </div>
                                <div className={styles["lbl-on-off"]} onClick={() => {
                                        setSelectedStatus_Off(true); 
                                        setUnselectedDates_Off({});
                                    }}>{t("on")}</div>
                            </div>
                        </div>
                        <div className={`${styles["frm-row"]} ${styles["h-auto"]} ${styles["frm-align-left"]}`}>
                            <div className={"txt-bold"}>{t("schedule")}:</div>
                        </div>
                        <div className={`${styles["frm-row"]} ${styles["h-auto"]} ${styles["frm-align-left"]}`}>
                            <input type="checkbox" id="chk-all-off" 
                                disabled={!selectedStatus_Off}
                                checked={selectedStatus_Off && isCurrectSelectAll(unselectedDates_Off)} 
                                onClick={() => {
                                    const isCheckAll = isCurrectSelectAll(unselectedDates_Off);
                                    const u = transferrableDates.map(() => isCheckAll);
                                    setUnselectedDates_Off(u);
                                }}/>

                            <label htmlFor={"chk-all-off"} className={styles["lbl-for-ckb"]}>{t("selectAll")}</label>
                        </div>
                        <div className={`${styles["frm-row"]} ${styles["h-auto"]} ${styles["frm-align-left"]} ${styles["flex-wrap"]}`}>
                            {
                                transferrableDates.length > 0 && transferrableDates.map((d: any, i: number) => {
                                    return (
                                        <div className={`${styles["flex"]} ${styles["align-center"]}`} key={i}>
                                            <input type="checkbox" id={`chk-${i}-off`}
                                                disabled={!selectedStatus_Off}
                                                checked={selectedStatus_Off && !unselectedDates_Off[i]} 
                                                onClick={() => { 
                                                    const u = {...unselectedDates_Off};
                                                    u[i] = !unselectedDates_Off[i]; 
                                                    setUnselectedDates_Off(u);
                                                }}/>
                                            <label htmlFor={`chk-${i}-off`} className={styles["lbl-for-ckb"]}>{t(`day${d}`)}</label>
                                        </div>
                                    );
                                })
                            }
                        </div>
                        <div className={`${styles["frm-row"]} ${styles["h-auto"]} ${styles["frm-align-left"]}`}>      
                            <div className={styles["div-usernames"]}>{offNames.join(", ")}</div>
                        </div>
                    </div>
                    { 
                        !updateResult && <ul className={styles["note-list-transfer"]}>
                            <li className={"text-note"}>{t("clearScheduleNote")}</li>
                            <li dangerouslySetInnerHTML={{__html: t("autoTransfer_InfoAffectTime")}}></li>
                            <li>{t("autoTransfer_SetSchedule")}</li>
                            <li>{t("autoTransfer_InfoCannotTransfer")}</li>
                        </ul>
                    }
                    <div className={`${styles["frm-row"]} ${styles["frm-align-right"]} ${styles["button-confirm"]}`}>                            
                        <button className="btn-cancel width-auto mt-0" type="button"
                            onClick={() => { returnCustomerLists(); }}>
                            {t("cancel")}
                        </button>                                           
                        <button className="btn-submit width-auto mt-0" type="submit"
                            disabled={!validateSchedule()}>
                            {t("ok")}
                        </button>                     
                    </div>
                </form>
            }
                
           
            {
                updateResult && <>
                    <form className={styles["form-auto-transfer-settings"]}>
                        <div className={`${styles["div-message"]} ${updateResult.hasError ? '': 'hide'}`}>
                            <Message message={updateResult.stopMessage || updateResult.errorMessage} isError={true}></Message>
                        </div>
                        <div className={`${styles["div-message"]} ${updateResult.hasWarning ? '': 'hide'}`}>
                            <Message message={updateResult.warningMessage} isError={false} isWarning={true}></Message>
                        </div>
                        <div className={`${styles["div-message"]} ${updateResult.isSuccess ? '': 'hide'}`}>
                            <Message message={updateResult.successMessage}></Message>
                        </div>
                        <div className={`${styles["frm-row"]} ${styles["frm-align-right"]} ${styles["button-confirm"]} ${canEditMore() ? '' : 'hide'}`}>                           
                                <button className="btn-submit" type="button"
                                onClick={() => { returnCustomerLists(true); }}>
                                {t("backToMainPage")}
                            </button>                            
                        </div>
                    </form>
                </>
            } 
        </>
    );
};


export default EditMultipleComponent;