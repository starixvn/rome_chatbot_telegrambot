namespace TelegramBot.Modules.AutoTransfer.Criteria
{
    public class GetMastersCriteria : GetCustomersCriteria
    {
        public int SAgId { get; set; }

        public override string GetSettingKey() => "Age_Transfer_SelList_Level4";

        public override bool IsValid() => SAgId > 0;
    }
}