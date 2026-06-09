namespace AuthService.Models;

public class LoginEvent
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public string Country { get; set; } = "";
    public bool IsSuccess { get; set; }
    public double AiScore { get; set; }
    public bool IsAnomaly { get; set; }
    public string Reason { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class LoginFeatures
{
    public string Username { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public int HourOfDay { get; set; }
    public int DayOfWeek { get; set; }
    public bool IsSuccess { get; set; }
    public string UserAgent { get; set; } = "";
    public string Country { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class AiScore
{
    public double Score { get; set; }
    public bool IsAnomaly { get; set; }
    public string Reason { get; set; } = "";
}