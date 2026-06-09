using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using AuthService.Data;
using AuthService.Hubs;
using AuthService.Models;
using AuthService.Services;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthDbContext _db;
    private readonly JwtService _jwt;
    private readonly BlockingService _blocking;
    private readonly AiClientService _ai;
    private readonly IHubContext<DashboardHub> _hub;

    public AuthController(
        AuthDbContext db, JwtService jwt,
        BlockingService blocking, AiClientService ai,
        IHubContext<DashboardHub> hub)
    {
        _db = db; _jwt = jwt;
        _blocking = blocking; _ai = ai; _hub = hub;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) ||
            string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Username dhe password kërkohen");

        if (_db.Users.Any(u => u.Username == req.Username))
            return BadRequest("Username ekziston");

        var user = new User
        {
            Username = req.Username,
            PasswordHash = BCrypt.Net.BCrypt
                .HashPassword(req.Password, workFactor: 12)
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Ok(new { message = "U regjistrua me sukses!" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest req)
    {
        // 1. Kontrollo bllokimin
        if (_blocking.IsBlocked(req.Username))
        {
            var until = _blocking.GetBlockedUntil(req.Username);
            return StatusCode(423, new
            {
                message = "Llogaria është bllokuar!",
                blockedUntil = until
            });
        }

        // 2. Verifiko userin
        var user = _db.Users.FirstOrDefault(
            u => u.Username == req.Username);
        bool success = user != null &&
            BCrypt.Net.BCrypt.Verify(
                req.Password, user.PasswordHash);

        // 3. IP dhe UserAgent
        string ip = !string.IsNullOrEmpty(req.IpAddress)
            ? req.IpAddress
            : HttpContext.Connection.RemoteIpAddress?
                .ToString() ?? "127.0.0.1";
        string ua = !string.IsNullOrEmpty(req.UserAgent)
            ? req.UserAgent
            : Request.Headers["User-Agent"].ToString();

        // 4. GeoIP
        string country = GetCountry(ip);

        // 5. Features për AI
        var features = new LoginFeatures
        {
            Username = req.Username,
            IpAddress = ip,
            HourOfDay = DateTime.UtcNow.Hour,
            DayOfWeek = (int)DateTime.UtcNow.DayOfWeek,
            IsSuccess = success,
            UserAgent = ua,
            Country = country,
            Latitude = GetLat(ip),
            Longitude = GetLon(ip),
            Timestamp = DateTime.UtcNow
        };

        // 6. AI Score
        var aiScore = await _ai.ScoreAsync(features);

        // 7. Ruaj event
        _db.LoginEvents.Add(new LoginEvent
        {
            Username = req.Username,
            IpAddress = ip,
            Country = country,
            IsSuccess = success,
            AiScore = aiScore.Score,
            IsAnomaly = aiScore.IsAnomaly,
            Reason = aiScore.Reason,
            Timestamp = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        // 8. Anomali → bllokon
        if (aiScore.IsAnomaly)
        {
            _blocking.Block(req.Username, 5);
            await _hub.Clients.Group("admins")
                .SendAsync("LoginAlert", new
                {
                    username = req.Username,
                    ip,
                    country,
                    score = aiScore.Score,
                    reason = aiScore.Reason,
                    blocked = true,
                    timestamp = DateTime.UtcNow
                });
            return StatusCode(423, new
            {
                message = "Aktivitet i dyshimtë — bllokuar 5 min!",
                score = aiScore.Score,
                reason = aiScore.Reason
            });
        }

        // 9. Alert normal
        await _hub.Clients.Group("admins")
            .SendAsync("LoginAlert", new
            {
                username = req.Username,
                ip,
                country,
                score = aiScore.Score,
                reason = aiScore.Reason,
                blocked = false,
                timestamp = DateTime.UtcNow
            });

        // 10. Login i dështuar
        if (!success)
            return Unauthorized(new
            {
                message = "Username ose password i gabuar"
            });

        // 11. JWT token
        var token = _jwt.GenerateToken(user!);
        return Ok(new
        {
            token,
            expiresIn = 1800,
            username = user!.Username
        });
    }

    [HttpGet("events")]
    public IActionResult GetEvents()
        => Ok(_db.LoginEvents
            .OrderByDescending(e => e.Timestamp)
            .Take(200).ToList());

    [HttpGet("blocked")]
    public IActionResult GetBlocked()
        => Ok(_blocking.GetAllBlocked());

    private string GetCountry(string ip) =>
        ip.StartsWith("193.") ? "Kosovo" :
        ip.StartsWith("177.") ? "Brazil" :
        ip.StartsWith("91.") ? "Albania" : "Unknown";

    private double GetLat(string ip) =>
        ip.StartsWith("193.") ? 42.6629 :
        ip.StartsWith("177.") ? -15.7801 : 0;

    private double GetLon(string ip) =>
        ip.StartsWith("193.") ? 21.1655 :
        ip.StartsWith("177.") ? -47.9292 : 0;
}