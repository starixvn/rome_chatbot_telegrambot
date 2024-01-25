using TelegramBot.Shared.Resources;

namespace TelegramBot.Modules.AutoTransfer.Models
{
    public class UpdateMultipleAutoTransferResultModel
    {
        public UpdateMultipleAutoTransferResultModel() { }

        public UpdateMultipleAutoTransferResultModel(int code, string stopMessage, string language)
        {
            Code = code;
            StopMessage = stopMessage;
            Language = language;
        }

        public string Language { get; set; }

        public int? Code { get; set; }

        public List<string> WarningCusts { get; set; } = new List<string>();

        public string StopMessage { get; set; }

        public string WarningMessage
        {
            get
            {
                var names =  string.Join(", ", WarningCusts);
                if (WarningCusts.Count > 1)
                { 
                    var message = ResourceHelper.Get(nameof(SharedResource.AutoTransfer_MultipleAffectNextConditionWarning), Language);
                    return $"{message}({names})";
                }

                if (WarningCusts.Count == 1)
                {
                    var message = ResourceHelper.Get(nameof(SharedResource.AutoTransfer_MultipleAffectNextConditionWarning_Single), Language);
                    return $"{message}({names})";
                }

                return string.Empty;
            }
        }

        public string ErrorMessage
        {
            get
            {
                var result = new List<string>();

                foreach (var err in ErrMessages)
                {
                    var custs = ErrCusts[err.Key];
                    var suffix = custs.Count > 1 ? "Multiple" : "Single";
                    var message = ResourceHelper.Get($"AutoTransfer_Error_{err.Key}_{suffix}", Language);
                    message = string.IsNullOrWhiteSpace(message) ? err.Value : message;

                    var names =  string.Join(", ", custs);
                    message += $"({names})";
                    result.Add(message);
                }

                return string.Join("<br />", result);
            }
        }

        public string SuccessMessage => ResourceHelper.Get(nameof(SharedResource.AutoTransfer_UpdateSuccess), Language);

        public Dictionary<string, string> ErrMessages { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, List<string>> ErrCusts { get; set; } = new Dictionary<string, List<string>>();

        public bool HasError => !string.IsNullOrWhiteSpace(StopMessage) || !string.IsNullOrWhiteSpace(ErrorMessage);
        public bool HasWarning => !string.IsNullOrWhiteSpace(WarningMessage);
        public bool IsSuccess => !HasError && !HasWarning;
    }
}