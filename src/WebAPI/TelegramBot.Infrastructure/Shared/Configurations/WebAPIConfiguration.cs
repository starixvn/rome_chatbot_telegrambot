using System.Collections.Generic;

namespace TelegramBot.Infrastructure.Shared.Configurations;

public class WebAPIConfiguration
{
    public int OtpExpirationInMinutes { get; set; }

    public int OtpMaxFailedTimes { get; set; }

    public bool IsProduction { get; set; }

    public int OutstandingMaxTopQuery { get; set; }

    public int BalanceMaxTopQuery { get; set; }

    public IDictionary<string, short[]> HiddenActionsBySystem { get; set; }
}
