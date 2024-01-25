using Fanex.Data;
using Fanex.Data.Repository;

namespace TelegramBot.Modules.AutoTransfer.Criteria
{
    public class GetJobStatusCriteria : NonQueryCommand
    {
         public GetJobStatusCriteria(int fromCustId, int toCustId, bool state, int siteDB)
        {
            FromCustId = fromCustId;
            ToCustId = toCustId;
            IsAllowAutoTransfer = state;
            ErrCode = 0;
            ErrMsg = string.Empty;
            SiteDBGroupID = siteDB.ToString();
        }

        public int FromCustId { get; }

        public int ToCustId { get; }

        public bool IsAllowAutoTransfer { get; }

        public bool IsAutoJobRunning { get; set; }

        public int ErrCode { get; set; }

        public string ErrMsg { get; set; }

        [SpParam(Ignored = true)]
        public string SiteDBGroupID { get; set; }

        public override string GetSettingKey() => $"SiteDB{SiteDBGroupID}_Age_ClearBalanceCust_Job_StatusCheck";

        public override bool IsValid() => true;
    }
}