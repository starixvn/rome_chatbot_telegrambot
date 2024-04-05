namespace TelegramBot.WebApi.Params.TotalBets
{
    public class TotalBetsQuery
    {
        public int CustId { get; set; }
        public int SubAccId { get; set; }   
        public int CustomerLevel { get; set; }   
        public string Language { get; set; }
        public bool IsApplyPT { get; set; }
    }
}