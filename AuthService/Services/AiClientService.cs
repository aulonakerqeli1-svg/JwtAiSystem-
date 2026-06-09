using System.Text;
using System.Text.Json;
using AuthService.Models;

namespace AuthService.Services;

public class AiClientService
{
    private readonly HttpClient _http = new();
    private readonly string _aiUrl;

    public AiClientService(IConfiguration cfg)
        => _aiUrl = cfg["AiModule:Url"]
           ?? "http://localhost:8000";

    public async Task<AiScore> ScoreAsync(LoginFeatures f)
    {
        try
        {
            var json = JsonSerializer.Serialize(f);
            var content = new StringContent(
                json, Encoding.UTF8, "application/json");
            var res = await _http.PostAsync(
                $"{_aiUrl}/score", content);
            var body = await res.Content
                .ReadAsStringAsync();
            return JsonSerializer.Deserialize<AiScore>(body,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new AiScore();
        }
        catch
        {
            return new AiScore
            {
                Score = 0,
                IsAnomaly = false,
                Reason = "AI offline"
            };
        }
    }
}