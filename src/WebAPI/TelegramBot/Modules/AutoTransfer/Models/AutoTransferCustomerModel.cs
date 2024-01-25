using TelegramBot.Modules.AutoTransfer.Consts;

namespace TelegramBot.Modules.AutoTransfer.Models
{
    public class AutoTransferCustomerModel
    {
        public decimal Balance { get; set; }
        public int RowNumber { get; set; }
        public string Username { get; set; }
        public decimal YesterdayBalance { get; set; }
        public int CustId { get; set; }

        public bool Enabled { get; set; }
        public bool Closed { get; set; }
        public bool Suspended { get; set; }
        public byte ClosedReason { get; set; }
        public byte Locked { get; set; }
        public bool EnableAutoTransfer { get; set; }
        public string AutoTransferSchedule { get; set; }

        public IEnumerable<string> AutoTransferDates 
            => string.IsNullOrWhiteSpace(AutoTransferSchedule) ? Enumerable.Empty<string>() : AutoTransferSchedule.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();
    
        public int Status { 
            get {
                if(!Enabled) {
                    return CustomerStatusConst.Disabled;
                }

                if(Closed) {
                    switch (ClosedReason)
                    {
                        case CustomerClosedReasonConst.ByAdmin:
                            return CustomerStatusConst.ClosedByAdmin;
                        case CustomerClosedReasonConst.ByUpline:
                            return CustomerStatusConst.ClosedByUpline;
                        case CustomerClosedReasonConst.ByArchive:
                            return CustomerStatusConst.ClosedByArchive;
                        default:
                            return CustomerStatusConst.Normal;
                    }
                }

                if(Suspended) {
                    return CustomerStatusConst.Suspended;
                }
                
                if(Locked > 0) {
                    switch (Locked)
                    {
                        case CustomerLockedReasonConst.FalseOTPSC:
                            return CustomerStatusConst.LockedByFalseOTPSC;
                        case CustomerLockedReasonConst.FalsePassword:
                            return CustomerStatusConst.LockedByFalsePassword;
                        default:
                            return CustomerStatusConst.Normal;
                    }
                }

                return CustomerStatusConst.Normal;
            }
        }
    }
}