using System;

namespace MemberAPIs.Models
{
    public class RateLimiterOptions
    {
        public bool AutoReplenishment { get; set; } = true;
        public int PermitLimit { get; set; } = 250;
        public int Window { get; set; } = 1;
        public string Unit { get; set; } = "s";
        public int SegmentsPerWindow { get; set; } = 1;

        public TimeSpan GetWindowTimeSpan() => Unit.ToLower() switch
        {
            "s" => TimeSpan.FromSeconds(Window),
            "m" => TimeSpan.FromMinutes(Window),
            "h" => TimeSpan.FromHours(Window),
            _ => TimeSpan.FromSeconds(Window)
        };
    }
} 