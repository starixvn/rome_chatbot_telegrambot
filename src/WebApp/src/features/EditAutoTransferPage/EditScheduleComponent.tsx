import styles from "./EditAutoTransferPage.module.scss";
import React, { useState } from "react";
import { useForm } from "react-hook-form";
import useTranslation from "next-translate/useTranslation";
import BlockUI from "@components/BlockUI";
import { fetchWrapper } from "src/lib/fetch-client";
import { useRouter } from "next/router";
import Message from "@components/Message";

type EditScheduleViewModel = {
    customer: any;
    uplineTransferOption: any;
    returnCustomerLists: any;
    transferrableDates: any;
};

type IFormInputs = {
    username: string;
};


const EditScheduleComponent = ({ 
    customer,
    uplineTransferOption,
    returnCustomerLists,
    transferrableDates,
    ...props
}: EditScheduleViewModel) => {
    const { t } = useTranslation("editautotransfer");
    const methods = useForm<IFormInputs>({
      mode: "onSubmit",
      defaultValues: { username: '' },
      shouldFocusError: false,
    });
    const { formState, handleSubmit, control } = methods;
    const router = useRouter();
    const [updateResult, setUpdateResult] = useState<any>();

    const initUnselectedDates: { [key: number]: boolean} = {};
    transferrableDates.map((d: any, i: number) => {
        initUnselectedDates[i] = !customer.autoTransferDates.some((a : any) => d == a);
    });

    const [unselectedDates, setUnselectedDates] = useState<{ [key: number]: boolean}>(initUnselectedDates);

    const onSubmit = handleSubmit(async () => {
        const url = `/api/autotransfer/updateSchedule`;
        const body = {
            custId: customer.custId,
            username: customer.username,
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
                    <div className={`${styles["frm-row"]}  ${styles["h-auto"]}`}>
                        <div className={"txt-bold"}>{t("confirmChangeAutoTransferSchedule").replace('{0}', customer.username)}</div>
                    </div>
                    <div className={`${styles["frm-row"]}  ${styles["h-auto"]}`}>
                        <div>{t("autoTransfer_SetSchedule")}</div>
                    </div>
                    <div className={`${styles["frm-row"]} ${styles["frm-align-left"]} ${styles["h-auto"]}`}>
                        <input type="checkbox" id="chk-all" checked={isCurrectSelectAll()} 
                            onClick={() => {
                                const isCheckAll = isCurrectSelectAll();
                                const u = transferrableDates.map(() => isCheckAll);
                                setUnselectedDates(u);
                            }}/>

                        <label htmlFor={"chk-all"} className={styles["lbl-for-ckb"]}>{t("selectAll")}</label>
                    </div>
                    <div className={`${styles["frm-row"]} ${styles["frm-align-left"]} ${styles["h-auto"]} ${styles["flex-wrap"]}`}>
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
                                        <label htmlFor={`chk-${i}`}  className={styles["lbl-for-ckb"]}>{t(`day${d}`)}</label>
                                    </div>
                                );
                            })
                        }
                    </div>
                    <div className={`${styles["frm-row"]} ${styles["frm-align-right"]} ${styles["button-confirm"]}`}>                                
                        <button className="btn-cancel width-auto" type="button"
                            onClick={() => { returnCustomerLists(); }}>
                            {t("cancel")}
                        </button>                       
                        <button className="btn-submit" type="submit"
                            disabled={!getSelectedSchedule()}>
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

export default EditScheduleComponent;