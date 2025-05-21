using System.Collections.Generic;

public class RateLimitConfigService
{
    public RateLimitConfig GetRateLimitConfig()
    {
        return new RateLimitConfig
        {
            RequestLimit = 100,
            TimeWindowSeconds = 30,
            BlockDurationMinutes = 30,
            IPRequestLimit = 300,  // Limit for IP-based tracking
            IPTimeWindowSeconds = 60, // 60 seconds for IP tracking
            WhitelistedHeaders = new List<string> { "FLUXTSC-Internal-Request" }
        };
    }
}

public class RateLimitConfig
{
    public int RequestLimit { get; set; }
    public int TimeWindowSeconds { get; set; }
    public int BlockDurationMinutes { get; set; }
    public int IPRequestLimit { get; set; }
    public int IPTimeWindowSeconds { get; set; }
    public List<string> WhitelistedHeaders { get; set; }
}