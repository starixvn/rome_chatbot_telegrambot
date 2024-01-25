using Fanex.Data;

namespace TelegramBot.Modules.AutoTransfer.Criteria
{
    public class GetMembersCriteria : GetCustomersCriteria
    {
        [SpParam(Ignored = true)]
        public string SiteDBGroupID { get; set; }

        public int CustId { get; set; }
        
        public override string GetSettingKey() => $"SiteDB{SiteDBGroupID}_Age_Transfer_SelList_Level2";

        public override bool IsValid() => CustId > 0;
    }
}