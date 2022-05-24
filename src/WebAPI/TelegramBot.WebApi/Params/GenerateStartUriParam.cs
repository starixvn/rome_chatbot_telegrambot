using System.ComponentModel.DataAnnotations;

namespace TelegramBot.WebApi.Params
{
    public class GenerateStartUriParam
    {
        [Required]
        public int CustId { get; set; }

        public int SubAccId { get; set; }

        [Required]
        public string System { get; set; }

        [Required]
        public string Language { get; set; }
    }
}
