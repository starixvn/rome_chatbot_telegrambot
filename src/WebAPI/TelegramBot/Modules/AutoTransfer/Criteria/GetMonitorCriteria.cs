using Fanex.Data;
using Fanex.Data.Repository;

namespace TelegramBot.Modules.AutoTransfer.Criteria
{
    public class GetMonitorCriteria : CriteriaBase
    {
        [SpParam(Ignored = true)]
        public string SiteDBGroupID { get; set; }

        public int FromCustId { get; set; }

        public string Language { get; set; }

        public string Searchname { get; set; }

        public string ViewStatus { get; set; }

        public int SortBy { get; set; }

        public int SortType { get; set; }

        public int PageSize { get; set; } = 100000;

        public int PageIndex { get; set; } = 1;

        public int TotalRow { get; set; }

        public int TotalCancelAll { get; set; }
        public int TotalPending { get; set; }
        public int TotalProcessing { get; set; }
        public int TotalFailed { get; set; }

        public override string GetSettingKey() => $"SiteDB{SiteDBGroupID}_Age_ClearBalanceCust_ProcessSel";

        public override bool IsValid() => FromCustId > 0;
    }
}