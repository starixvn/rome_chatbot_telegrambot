using Fanex.Data.Repository;

namespace TelegramBot.Modules.AutoTransfer.Criteria
{
    public class UpdateStatusCriteria : NonQueryCommand
    {
        public int FromCustId { get; set; }

        public int ToCustId { get; set; }

        public bool IsAutoJobRunning { get; set; }

        public bool IsAllowAutoTransfer { get;  set; }

        public string AutoTransferSchedule { get; set; }

        public int SubAccId { get; set; }

        public int Creator  => FromCustId;

        public string Language { get; set; }

        public string IP { get; set; }

        public string  Old_TransferOption { get; set; }

        public int ErrCode { get; set; }

        public string ErrMsg { get; set; }
      
        public override string GetSettingKey() => "Age_ClearBalanceCust_SettingUpdate";

        public override bool IsValid() => FromCustId > 0 && ToCustId > 0;
    }
}