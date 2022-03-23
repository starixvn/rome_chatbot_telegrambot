using System.ComponentModel.DataAnnotations;

namespace TelegramBot.WebApi.Params
{
    public class GenerateStartUriParam
    {
        [Required]
        public string System { get; set; }

        [Required]
        public bool IsTest { get; set; }
    }
}
