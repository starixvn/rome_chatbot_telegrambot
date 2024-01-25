
import styles from "./EditAutoTransferPage.module.scss";
import React, { useState, useEffect } from "react";
import { useRouter } from "next/router";
import { useForm } from "react-hook-form";
import { User } from "src/lib/session";
import useTranslation from "next-translate/useTranslation";
import { fetchWrapper } from "src/lib/fetch-client";
import BlockUI from "@components/BlockUI";
import { SingleSelect } from "@components/Form/Select";
import Message from "@components/Message";
import { FirstPageIcon, PrevPageIcon, NextPageIcon, LastPageIcon } from "@components/Icons";
import CustomerListComponent from "./CustomerListComponent";
import EditStatusComponent from "./EditStatusComponent";
import EditScheduleComponent from "./EditScheduleComponent";
import EditMultipleComponent from "./EditMultipleComponent";

type EditAutoTransferViewModel = {
    user: User;
};

type IFormInputs = {
    username: string;
    status: string;
    autoTransfer: string;
};

type CustomerListDataViewModel = {
    customers: any;
    uplineTransferOption: any;
    transferrableDates: any;
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

export const EditAutoTransferPage = ({ user, ...props }: EditAutoTransferViewModel) => {
    const adbtChangedCode = 108;
    const status = {
        all: 0,
        open: 1,
        suspended: 2,
        closed: 3,
        disabled: 4
    };
    const pageLevels = {
        customerList: 0,
        editStatus: 1,
        editSchedule: 2,
        editMultiple: 3
    };

    const router = useRouter();
    const methods = useForm<IFormInputs>({
      mode: "onSubmit",
      defaultValues: { username: '', status: '' },
      shouldFocusError: false,
    });

    const { t } = useTranslation("editautotransfer");
    const { formState, handleSubmit, control } = methods;
    const [errorMessage, setErrorMessage] = useState("");
    const [errorCode, setErrorCode] = useState(0);
    const [username, setUsername] = useState("");
    const [selectedStatus, setSelectedStatus] = useState("0");
    const [autoTransferStatus, setAutoTransferStatus] = useState("2");
    const [pageIndex, setPageIndex] = useState(1);

    const [pageSize, setPageSize] = useState("50");
    const [customerListData, setCustomerListData] = useState<CustomerListDataViewModel>();
    const [expandStatus, setExpandStatus] = useState<{ [key: number]: boolean}>({});

    const [pageLevel, setPageLevel] = useState(pageLevels.customerList);
    const [selectedCustomer, setSelectedCustomer] = useState();
    const [isInitPager, setIsInitPager] = useState(false);
    const [selectedCustomers, setSelectedCustomers] = useState<{ [key: number]: boolean}>({});
    const [editingPageIndex, setEditingPageIndex] = useState<number | undefined>(1);
    const [isExpandAll, setIsExpandAll] = useState(false);

    useEffect(
        () => { 
            setEditingPageIndex(pageIndex);
            pagingCustomers(); 
            setIsInitPager(true); 
        }, 
        [pageSize, pageIndex]);

    function resetCustomersGrid() {
        setIsInitPager(false);
        setExpandStatus({});
        setIsExpandAll(false);
        setSelectedCustomers({});
        setCustomerListData(undefined);
        setPageIndex(1);
        setPageSize("50");
        setIsInitPager(true);
    }

    const onSubmit = handleSubmit(async () => {
        resetCustomersGrid();
        const url = `/api/autotransfer/customers`;
        const body = {
            custId: user?.custId,
            language: user?.language,
            custLevel: user?.customerLevel,
            username: username,
            status: selectedStatus,
            autoTransferStatus: autoTransferStatus,
            pageIndex: 1,
            pageSize: 50,
        };

        const data = await fetchWrapper.post<any>(url, body, router);
        setCustomersResponse(data);
    });

    const pagingCustomers = handleSubmit(async () => {
        if(!isInitPager) {
            return;
        }

        setExpandStatus({});
        setSelectedCustomers({});
        setCustomerListData(undefined);
        const url = `/api/autotransfer/customers`;
        const body = {
            custId: user?.custId,
            language: user?.language,
            custLevel: user?.customerLevel,
            username: username,
            status: selectedStatus,
            autoTransferStatus: autoTransferStatus,
            pageIndex: pageIndex,
            pageSize: pageSize,
        };

        const data = await fetchWrapper.post<any>(url, body, router);
        setCustomersResponse(data);
    });

    function setCustomersResponse(data: any) {
        if (data.message) {
            setErrorMessage(data.message);
            setErrorCode(data.code);
        } else {
            setCustomerListData(data);
        }
    }

    function generateStatus() {
        return [
            { value: status.all, displayName: t("all") },
            { value: status.open, displayName: t("open") },
            { value: status.suspended, displayName: t("suspended") },
            { value: status.closed, displayName: t("closed") },
            { value: status.disabled, displayName: t("disabled") },
        ];
    }

    function generateAutoTransferStatus() {
        return [
            { value: 2, displayName: t("all") },
            { value: 1, displayName: t("enabled") },
            { value: 0, displayName: t("disabled") },
        ];
    }

    function generatePageSize() {
        const pageSizes = [50, 100, 200];

        return pageSizes.map((s) => {
            return { value: s, displayName: s.toString() };
        });
    }

    function editStatus(customer: any) {
        setSelectedCustomer(customer);
        setPageLevel(pageLevels.editStatus);
    }

    function editSchedule(customer: any) {
        setSelectedCustomer(customer);
        setPageLevel(pageLevels.editSchedule);
    }

    function editMultiple() {
        setPageLevel(pageLevels.editMultiple);
    }

    function returnCustomerLists(reloadPage: boolean) {
        setPageLevel(pageLevels.customerList);

        if(reloadPage) {
            onSubmit();
        }
    }

    return (
        <>
            {
                pageLevel === pageLevels.customerList && <>
                    {formState.isSubmitting && <BlockUI />}
                    {  
                        errorCode != adbtChangedCode && <form className={styles["form-auto-transfer-settings"]} onSubmit={onSubmit}>
                            <div className={styles["frm-row-label"]}>
                                <div className={`${styles["div-half"]} txt-bold`}>{t("username")}</div>
                                <div className={`${styles["div-half"]} txt-bold`}>{t("status")}</div>
                            </div>
                            <div className={styles["frm-row"]}>
                                <div className={styles["div-half"]}>
                                    <input type="text" name="username" 
                                        className={styles["txt-username"]}
                                        onChange={(e) => { setUsername(e.target.value); }} />
                                </div>

                                <div className={styles["div-half"]}>
                                    <SingleSelect
                                        options={generateStatus()}
                                        selectedValue={selectedStatus}
                                        handleChange={(e: React.ChangeEvent<HTMLSelectElement>) => {
                                            setSelectedStatus(e.target.value);
                                        }}
                                        />
                                </div>
                            </div>
                            <div className={styles["frm-row-label"]}>
                                <div className={`${styles["div-half"]} txt-bold`}>{t("autoTransfer")}</div>
                                <div className={styles["div-half"]}>&nbsp;</div>
                            </div>
                            <div className={styles["frm-row"]}>
                                <div className={styles["div-half"]}>
                                    <SingleSelect
                                        options={generateAutoTransferStatus()}
                                        selectedValue={autoTransferStatus}
                                        handleChange={(e: React.ChangeEvent<HTMLSelectElement>) => {
                                            setAutoTransferStatus(e.target.value);
                                        }}
                                        />
                                </div>                                
                                <div className={styles["div-half"]}>
                                    <button className="btn-submit width-auto mt-0" type="submit" disabled={!formState.isValid}>
                                        {t("submit")}
                                    </button>
                                </div>
                            </div>
                            <div className={styles["frm-row"]}>                                
                                <button className={"btn-submit width-auto mt-0"} type="button" 
                                    disabled={!customerListData || !customerListData.customers.length || !customerListData.customers.some((_: any, i: number) => selectedCustomers[i])}
                                    onClick={() => { editMultiple(); }}>
                                    {t("multipleAutoTransfer")}
                                </button>
                                
                            </div>
                            {customerListData && customerListData.customers.length > 0 && <>
                                <div className={styles["frm-row"]}>
                                    <div className={`${styles["div-page-index"]}`}>
                                        <div className={`${styles["div-icon-paging"]} ${ pageIndex==1 ? styles["disabled"] : ""}`}
                                            onClick={() => { pageIndex > 1 && setPageIndex(1); }}>
                                            <FirstPageIcon height={16} width={16} />
                                        </div>
                                        <div className={`${styles["div-icon-paging"]} ${styles["div-page-prev"]} ${ pageIndex==1 ? styles["disabled"] : ""}`}
                                            onClick={() => { pageIndex > 1 && setPageIndex(pageIndex - 1); }}>
                                            <PrevPageIcon height={16} width={16} />
                                        </div>
                                        <span>{t("pageIndex")}</span>
                                        <input type="text" name="pageIndex" 
                                            className={styles["page-index"]}
                                            onKeyDown ={(e) => { 
                                                if (e.key !== 'Enter') {
                                                    return;
                                                }

                                                if(!editingPageIndex) {
                                                    setEditingPageIndex(1);
                                                }
                                                
                                                setPageIndex(editingPageIndex || 1); 
                                            }}
                                            onBlur={() => { 
                                                if(!editingPageIndex) {
                                                    setEditingPageIndex(1);
                                                }

                                                setPageIndex(editingPageIndex || 1); 
                                            }}
                                            onChange={(e) => { 
                                                if(!e.target.value) {
                                                    setEditingPageIndex(undefined); 
                                                    return;
                                                }

                                                const n = Number(e.target.value);
                                                if(n) {
                                                    setEditingPageIndex(n);    
                                                } else {
                                                    e.target.value = (editingPageIndex || "").toString();
                                                }
                                            }} 
                                            maxLength={6}
                                            value={editingPageIndex || ""} />
                                        <div>{t("of")} {customerListData.totalPages}</div>
                                        <div className={`${styles["div-icon-paging"]} ${styles["div-page-next"]} ${ pageIndex==customerListData.totalPages ? styles["disabled"] : ""}`}
                                            onClick={() => { pageIndex < customerListData.totalPages && setPageIndex(pageIndex + 1); }}>
                                            <NextPageIcon height={16} width={16} />
                                        </div>
                                        <div className={`${styles["div-icon-paging"]} ${ pageIndex==customerListData.totalPages ? styles["disabled"] : ""}`}
                                            onClick={() => { pageIndex < customerListData.totalPages && setPageIndex(customerListData.totalPages); }}>
                                            <LastPageIcon height={16} width={16} />
                                        </div>
                                    </div>
                                    
                                    <div className={`${styles["div-page-size"]}`}>
                                        <span>{t("pageSize")}</span>
                                        <SingleSelect 
                                            className={styles["ddl-page-size"]}
                                            options={generatePageSize()}
                                            selectedValue={pageSize}
                                            handleChange={(e: React.ChangeEvent<HTMLSelectElement>) => {
                                                setPageSize(e.target.value);
                                            }}
                                            />
                                    </div>
                                </div>
                            </>}
                        </form>
                    }
                    {errorMessage && <Message message={errorMessage} isError></Message>}
                    {
                        customerListData && <CustomerListComponent
                            {...customerListData} 
                            expandStatus={expandStatus}
                            setExpandStatus={setExpandStatus}
                            isExpandAll={isExpandAll}
                            setIsExpandAll={setIsExpandAll}
                            editStatus={editStatus}
                            editSchedule={editSchedule}
                            selectedCustomers={selectedCustomers}
                            setSelectedCustomers={setSelectedCustomers}
                            />
                    }
                </>
            }
            {
                pageLevel === pageLevels.editStatus && <>
                    <EditStatusComponent customer={selectedCustomer}
                        uplineTransferOption={customerListData?.uplineTransferOption}
                        returnCustomerLists={returnCustomerLists}
                        transferrableDates={customerListData?.transferrableDates}
                    ></EditStatusComponent>
                </>
            }
            {
                pageLevel === pageLevels.editSchedule && <>
                    <EditScheduleComponent customer={selectedCustomer}
                        uplineTransferOption={customerListData?.uplineTransferOption}
                        returnCustomerLists={returnCustomerLists}
                        transferrableDates={customerListData?.transferrableDates}
                    ></EditScheduleComponent>
                </>
            }
            {
                pageLevel === pageLevels.editMultiple && <>
                    <EditMultipleComponent
                        uplineTransferOption={customerListData?.uplineTransferOption}
                        returnCustomerLists={returnCustomerLists}
                        transferrableDates={customerListData?.transferrableDates}
                        onCustomers={customerListData ? customerListData.customers.filter((c: any, i: number) => selectedCustomers[i] && c.enableAutoTransfer) : []}
                        offCustomers={customerListData ? customerListData.customers.filter((c: any, i: number) => selectedCustomers[i] && !c.enableAutoTransfer) : []}
                    ></EditMultipleComponent>
                </>
            }
        </>
    );
};