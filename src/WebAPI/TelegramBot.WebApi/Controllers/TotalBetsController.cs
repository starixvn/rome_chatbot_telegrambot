using Alpha.API.Client.Models.TotalBets;
using Microsoft.AspNetCore.Mvc;
using TelegramBot.Modules.TotalBets;
using TelegramBot.WebApi.Params.TotalBets;

namespace TelegramBot.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TotalBetsController : ControllerBase
    {
        private readonly ITotalBetsHandler handler;

        public TotalBetsController(ITotalBetsHandler handler)
        {
            this.handler = handler;
        }

        [Route("Summary"), HttpPost]
        public async Task<TotalBetsModel> Summary([FromBody] TotalBetsQuery query)
            => await handler.Summary(
                query.CustId, 
                query.SubAccId, 
                query.CustomerLevel, 
                query.Language,
                query.IsApplyPT);

        [Route("Details"), HttpPost]
        public async Task<TotalBetsDetailsModel> Details([FromBody] TotalBetsDetailsQuery query)
            => await handler.Details(
                query.CustId, 
                query.SubAccId, 
                query.CustomerLevel, 
                query.Language, 
                query.MatchId,
                query.IsApplyPT);
    }
}