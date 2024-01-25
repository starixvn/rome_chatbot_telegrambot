namespace TelegramBot.Modules.AutoTransfer.Criteria
{
    public class GetAgentsCriteria : GetCustomersCriteria
    {
        public int MasterId { get; set; }
      
        public override string GetSettingKey() => "Age_Transfer_SelList_Level3";

        public override bool IsValid() => MasterId > 0;
    }
}