using Fanex.Data;

namespace TelegramBot.Infrastructure.Shared
{
    public class ResultCriteriaBase
    {
        public int ErrCode { get; set; }

        public string ErrMsg { get; set; }

        [SpParam(Ignored = true)]
        public bool IsSuccess => ErrCode == 0;
    }
}