using Alpha.API.Client.Models.TotalBets;
using Alpha.API.Client.Services;

namespace TelegramBot.Modules.TotalBets
{
    public interface ITotalBetsHandler
    {
        Task<TotalBetsModel> Summary(int custId, int subAccId, int customerLevel, string language, bool isApplyPT);
        Task<TotalBetsDetailsModel> Details(int custId, int subAccId, int customerLevel, string language, int matchId, bool isApplyPT);
    }

    public class TotalBetsHandler : ITotalBetsHandler
    {
        private readonly ITotalBetsService totalBetsService;

        public TotalBetsHandler(ITotalBetsService totalBetsService)
        {
            this.totalBetsService = totalBetsService;
        }

        public Task<TotalBetsModel> Summary(int custId, int subAccId, int customerLevel, string language, bool isApplyPT)
        {
            return totalBetsService.Summary(custId, subAccId, customerLevel, language, isApplyPT);
        }

        public Task<TotalBetsDetailsModel> Details(int custId, int subAccId, int customerLevel, string language, int matchId, bool isApplyPT)
        {
            return totalBetsService.Details(custId, subAccId, customerLevel, language, matchId, isApplyPT);
        }
    }
}