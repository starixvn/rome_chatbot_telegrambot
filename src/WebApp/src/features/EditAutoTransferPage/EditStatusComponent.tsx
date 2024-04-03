import styles from "./EditAutoTransferPage.module.scss";
import React, { useState } from "react";
import { useForm } from "react-hook-form";
import useTranslation from "next-translate/useTranslation";
import BlockUI from "@components/BlockUI";
import { fetchWrapper } from "src/lib/fetch-client";
import { useRouter } from "next/router";
import Message from "@components/Message";

type EditStatusViewModel = {
    customer: any;
    uplineTransferOption: any;
    returnCustomerLists: any;
    transferrableDates: any;
};

type IFormInputs = {
    username2: string;
};

const EditStatusComponent = ({ 
    customer,
    uplineTransferOption,
    returnCustomerLists,
    transferrableDates,
    ...props
}: EditStatusViewModel) => {
    const { t } = useTranslation("editautotransfer");
    const methods = useForm<IFormInputs>({
      mode: "onSubmit",
      defaultValues: { username2: '' },
      shouldFocusError: false,
    });

    const { formState, handleSubmit, control } = methods;
    const router = useRouter();
    const [updateResult, setUpdateResult] = useState<any>();
    const [unselectedDates, setUnselectedDates] = useState<{ [key: number]: boolean}>({});

    const onSubmit = handleSubmit(async () => {
        const url = `/api/autotransfer/updateStatus`;

        const body = {
            custId: customer.custId,
            username: customer.username,
            state: !customer.enableAutoTransfer,
            schedule: getSelectedSchedule(),
            oldTransferOption: uplineTransferOption
        };

        const data = await fetchWrapper.post<any>(url, body, router);
        setUpdateResult(data);
    });

    function isCurrectSelectAll() {
        return !transferrableDates.some((_: any, i: number) => unselectedDates[i]);
    }

    function getSelectedSchedule() {
        const schedule = transferrableDates.filter((_: any, i: number) => !unselectedDates[i]);
        return schedule.join(',');
    }

    function canEditMore() {
        return updateResult && updateResult.code != 108;
    }

    return (
        <>
            {formState.isSubmitting && <BlockUI />}
            {
                !updateResult  && <form className={styles["form-auto-transfer-settings"]} onSubmit={onSubmit}>
                    {
                        customer.enableAutoTransfer && <>
                            <div className={styles["frm-row"]}>
                                <div className={"txt-bold"}>{t("confirmTurnOffAutoTransfer")} {customer.username}?</div>
                            </div>
                            <div className={styles["frm-row"]}>
                                <div className={"txt-bold text-note"}>*{t("clearScheduleNote")}</div>
                            </div>
                        </>
                    }
                    {
                        !customer.enableAutoTransfer && <>
                            <div className={`${styles["frm-row"]} ${styles["h-auto"]} ${styles["frm-align-left"]}`}>
                                <div className={"txt-bold"}>{t("confirmTurnOnAutoTransfer")} {customer.username}?</div>
                            </div>
                            <div className={`${styles["frm-row"]} ${styles["h-auto"]} ${styles["frm-align-left"]}`}>
                                <div dangerouslySetInnerHTML={{__html: t("autoTransfer_InfoAffectTime")}}></div>
                            </div>
                            <div className={`${styles["frm-row"]} ${styles["h-auto"]} ${styles["frm-align-left"]}`}>
                                <div>{t("autoTransfer_SetSchedule")}</div>
                            </div>
                            <div className={`${styles["frm-row"]} ${styles["h-auto"]} ${styles["frm-align-left"]}`}>
                                <input type="checkbox" id="chk-all" checked={isCurrectSelectAll()} 
                                    onClick={() => {
                                        const isCheckAll = isCurrectSelectAll();
                                        const u = transferrableDates.map(() => isCheckAll);
                                        setUnselectedDates(u);
                                    }}/>

                                <label htmlFor={"chk-all"} className={styles["lbl-for-ckb"]}>{t("selectAll")}</label>
                            </div>
                            <div className={`${styles["frm-row"]} ${styles["h-auto"]} ${styles["frm-align-left"]} ${styles["flex-wrap"]}`}>
                                {
                                    transferrableDates.length > 0 && transferrableDates.map((d: any, i: number) => {
                                        return (
                                            <div className={`${styles["flex"]} ${styles["align-center"]}`} key={i}>
                                                <input type="checkbox" id={`chk-${i}`} checked={!unselectedDates[i]} 
                                                    onClick={() => { 
                                                        const u = {...unselectedDates};
                                                        u[i] = !unselectedDates[i]; 
                                                        setUnselectedDates(u);
                                                    }}/>
                                                <label htmlFor={`chk-${i}`} className={styles["lbl-for-ckb"]}>{t(`day${d}`)}</label>
                                            </div>
                                        );
                                    })
                                }
                            </div>
                        </>
                    }
                    <div className={`${styles["frm-row"]} ${styles["frm-align-right"]} ${styles["button-confirm"]}`}>
                        <button className="btn-cancel width-auto" type="button"
                            onClick={() => { returnCustomerLists(); }}>
                            {t("cancel")}
                        </button>                           
                        <button className="btn-submit width-auto" type="submit" 
                            disabled={!customer.enableAutoTransfer && !getSelectedSchedule()}>
                            {t("ok")}
                        </button>                         
                    </div>
                </form>
            }
            {
                updateResult && <>
                    <form className={styles["form-auto-transfer-settings"]}>
                        <div className={styles["div-message"]}>
                            <Message message={updateResult.message} isError={updateResult.code > 0} isWarning={updateResult.code < 0}></Message>
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

export default EditStatusComponent;