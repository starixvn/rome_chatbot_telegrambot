import { useRouter } from "next/router";
import BlockUI from "@components/BlockUI";
import React, { useState, useEffect } from "react";
import { useForm } from "react-hook-form";
import useTranslation from "next-translate/useTranslation";
import styles from "./AutoTransferMonitorPage.module.scss";
import { fetchWrapper } from "src/lib/fetch-client";
import { User } from "src/lib/session";
import MultiSelect, { Option } from "@components/Form/MultiSelect";
import Message from "@components/Message";
import MonitorComponent from "./MonitorComponent";

type AutoTransferMonitorViewModel = {
  user: User;
};

type IFormInputs = {
  username: string;
  status: string;
};

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
  isExpandAll: any;
  setIsExpandAll: any;
  setExpandStatus: any;
};

export const AutoTransferMonitorPage = ({ user, ...props }: AutoTransferMonitorViewModel) => {
  const adbtChangedCode = 108;
  const router = useRouter();
  const methods = useForm<IFormInputs>({
    mode: "onSubmit",
    defaultValues: { username: "", status: "" },
    shouldFocusError: false,
  });

  const { formState, handleSubmit, control } = methods;
  const { t } = useTranslation("viewautotransfermonitor");
  const [username, setUsername] = useState("");

  function getStatusOptions() {
    return [
      { value: "New,Waiting,Processing", label: t("processing") },
      { value: "Completed", label: t("completed") },
      { value: "Failed", label: t("failed") },
      { value: "Cancelled", label: t("cancelled") },
      { value: "Pending", label: t("pending") },
      { value: "Ignored", label: t("ignored") },
    ];
  }

  const statusOptions = getStatusOptions();
  const [selectedStatus, setSelectedStatus] = useState<Option[] | null>(statusOptions);
  const handleStatusChange = (selected: Option[]) => {
    setSelectedStatus(selected);
  };

  const [errorMessage, setErrorMessage] = useState("");
  const [errorCode, setErrorCode] = useState(0);
  const [monitorData, setMonitorData] = useState<MonitorDataViewModel>();
  const [sortedBy, setSortedBy] = useState(1);
  const [sortedDirection, setSortedDirection] = useState(0);
  const [canGetData, setCanGetData] = useState(false);
  const [sortValue, setSortValue] = useState(sortedBy * 10 + sortedDirection);
  const [expandStatus, setExpandStatus] = useState<{ [key: number]: boolean }>({});
  const [isExpandAll, setIsExpandAll] = useState(false);

  useEffect(() => {
    onSubmit();
    setCanGetData(true);
  }, [sortValue]);

  const onSubmit = handleSubmit(async () => {
    if (!canGetData) {
      return;
    }

    const url = `/api/autotransfer/monitor`;
    const body = {
      custId: user?.custId,
      language: user?.language,
      custLevel: user?.customerLevel,
      username: username,
      status: selectedStatus?.map((s) => s.value).join(","),
      sortedBy: sortedBy,
      direction: sortedDirection,
    };

    const data = await fetchWrapper.post<any>(url, body, router);

    if (data.message) {
      setErrorMessage(data.message);
      setErrorCode(data.code);
    } else {
      setExpandStatus({});
      setIsExpandAll(false);
      setMonitorData(data);
    }
  });

  return (
    <>
      {formState.isSubmitting && <BlockUI />}
      {errorCode != adbtChangedCode && (
        <form className={styles["form-monitor"]} onSubmit={onSubmit}>
          <div className={styles["frm-row-label"]}>
            <div className={`${styles["div-half"]} txt-bold`}>{t("username")}</div>
            <div className={`${styles["div-half"]} txt-bold`}>{t("status")}</div>
          </div>
          <div className={styles["frm-row"]}>
            <div className={styles["div-half"]}>
              <input
                type="text"
                name="username"
                className={styles["username"]}
                onChange={(e) => {
                  setUsername(e.target.value);
                }}
              />
            </div>

            <div className={styles["div-half"]}>
              <div className={styles["multiSelect-control"]}>
                <MultiSelect
                  key="sel-status"
                  options={getStatusOptions()}
                  onChange={handleStatusChange}
                  value={selectedStatus}
                  isSelectAll={true}
                  menuPlacement={"bottom"}
                  styles={{ control: { margin: "0 2px" } }}
                />
              </div>
            </div>
          </div>
          <div className={styles["frm-row"]}>
            <div className={styles.row}>
              <button className="btn-submit auto-transfer" type="submit" disabled={!formState.isValid}>
                {t("submit")}
              </button>
            </div>
          </div>
        </form>
      )}
      {errorMessage && <Message message={errorMessage} isError></Message>}
      {errorCode != adbtChangedCode && monitorData && (
        <MonitorComponent
          {...monitorData}
          sortedBy={sortedBy}
          sortedDirection={sortedDirection}
          setSortedBy={setSortedBy}
          setSortedDirection={setSortedDirection}
          expandStatus={expandStatus}
          setExpandStatus={setExpandStatus}
          isExpandAll={isExpandAll}
          setIsExpandAll={setIsExpandAll}
          setSortValue={setSortValue}
        />
      )}
    </>
  );
};
