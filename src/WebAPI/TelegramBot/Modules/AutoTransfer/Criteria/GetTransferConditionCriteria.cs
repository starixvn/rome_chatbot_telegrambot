using Fanex.Data.Repository;

namespace TelegramBot.Modules.AutoTransfer.Criteria
{
    public class GetTransferConditionCriteria : NonQueryCommand
    {
        public GetTransferConditionCriteria(int custId)
            => CustId = custId;

        public int CustId { get; set; }
        public string TransferOption { get; set; }
      
        public override string GetSettingKey() => "Acc_CustomerSelTransferOption";

        public override bool IsValid() => CustId > 0;
    }
}