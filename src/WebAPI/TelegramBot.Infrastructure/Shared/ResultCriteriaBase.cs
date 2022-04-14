using Fanex.Data;
using TelegramBot.Client.Constants;

namespace TelegramBot.Infrastructure.Shared
{
    public class ResultCriteriaBase
    {
        public int ErrCode { get; set; }

        public string ErrMsg { get; set; }

        [SpParam(Ignored = true)]
        public bool IsSuccess => ErrCode == ErrorCodes.Success;
    }
}
