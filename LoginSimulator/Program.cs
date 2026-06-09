using System.Text;
using System.Text.Json;

var http = new HttpClient();
var authUrl = "http://localhost:5001";

Console.WriteLine("╔══════════════════════════════════╗");
Console.WriteLine("║      JWT AI Login Simulator       ║");
Console.WriteLine("╠══════════════════════════════════╣");
Console.WriteLine("║  1 → Regjistro testuser           ║");
Console.WriteLine("║  2 → Brute Force (100 gabim)      ║");
Console.WriteLine("║  3 → Impossible Travel            ║");
Console.WriteLine("║  4 → Login Normal                 ║");
Console.WriteLine("║  5 → Test JWT (401/200/401)       ║");
Console.WriteLine("╚══════════════════════════════════╝");
Console.Write("\nZgjidh: ");

switch (Console.ReadLine())
{
    case "1": await Register(); break;
    case "2": await BruteForce(); break;
    case "3": await ImpossibleTravel(); break;
    case "4": await NormalLogin(); break;
    case "5": await TestJwt(); break;
    default: Console.WriteLine("E gabuar!"); break;
}

// ── Register ───────────────────────────────────────
async Task Register()
{
    Console.WriteLine("\n[Register] Duke regjistruar...");
    var res = await Post($"{authUrl}/api/auth/register",
        new { username = "testuser", password = "password123" });
    Console.WriteLine($"Status:   {res.status}");
    Console.WriteLine($"Response: {res.body}");
}

// ── Brute Force ────────────────────────────────────
async Task BruteForce()
{
    Console.WriteLine(
        "\n[BruteForce] 100 login të gabuar...\n");

    for (int i = 1; i <= 100; i++)
    {
        var res = await Post($"{authUrl}/api/auth/login",
            new
            {
                username = "testuser",
                password = $"wrongpass{i}",
                ipAddress = "193.0.0.1",
                userAgent = "Mozilla/5.0 Chrome/120"
            });

        Console.WriteLine(
            $"[{i:000}/100] HTTP {res.status} | " +
            $"{res.body[..Math.Min(70, res.body.Length)]}");

        if (res.status == 423)
        {
            Console.WriteLine(
                "\n🚫 LLOGARIA U BLLOKUA!" +
                " AI e zbuloi brute-force!");
            break;
        }
        await Task.Delay(600);
    }
}

// ── Impossible Travel ──────────────────────────────
async Task ImpossibleTravel()
{
    Console.WriteLine("\n[Travel] Login nga Kosova...");
    var r1 = await Post($"{authUrl}/api/auth/login",
        new
        {
            username = "testuser",
            password = "password123",
            ipAddress = "193.0.0.1",
            userAgent = "Mozilla/5.0 Chrome/120"
        });
    Console.WriteLine($"Kosovë: HTTP {r1.status}");
    Console.WriteLine($"Response: {r1.body}");

    Console.WriteLine("\n⏳ Duke pritur 10 sekonda...");
    for (int i = 10; i > 0; i--)
    {
        Console.Write($"\r{i}s mbetur...  ");
        await Task.Delay(1000);
    }

    Console.WriteLine("\n\n[Travel] Login nga Brazili...");
    var r2 = await Post($"{authUrl}/api/auth/login",
        new
        {
            username = "testuser",
            password = "password123",
            ipAddress = "177.0.0.1",
            userAgent = "curl/7.68.0"
        });
    Console.WriteLine($"Brazil: HTTP {r2.status}");
    Console.WriteLine($"Response: {r2.body}");
}

// ── Normal Login ───────────────────────────────────
async Task NormalLogin()
{
    Console.WriteLine("\n[Normal] Login normal...");
    var res = await Post($"{authUrl}/api/auth/login",
        new
        {
            username = "testuser",
            password = "password123",
            ipAddress = "193.0.0.1",
            userAgent = "Mozilla/5.0 Chrome/120"
        });
    Console.WriteLine($"Status:   {res.status}");
    Console.WriteLine($"Response: {res.body}");
}

// ── Test JWT ───────────────────────────────────────
async Task TestJwt()
{
    // 1. Pa token → 401
    Console.WriteLine("\n[JWT] 1. Pa token → duhet 401");
    var r1 = await http.GetAsync(
        "http://localhost:5002/api/data/protected");
    Console.WriteLine($"Result: HTTP {(int)r1.StatusCode}");

    // 2. Merr token valid
    Console.WriteLine("\n[JWT] 2. Duke marrë token...");
    var login = await Post($"{authUrl}/api/auth/login",
        new
        {
            username = "testuser",
            password = "password123",
            ipAddress = "193.0.0.1",
            userAgent = "Mozilla/5.0 Chrome/120"
        });

    string token = "";
    try
    {
        var doc = JsonDocument.Parse(login.body);
        token = doc.RootElement
            .GetProperty("token").GetString() ?? "";
        Console.WriteLine($"Token: {token[..50]}...");
    }
    catch
    {
        Console.WriteLine("Login dështoi!");
        return;
    }

    // 3. Me token valid → 200
    Console.WriteLine("\n[JWT] 3. Me token valid → duhet 200");
    var req2 = new HttpRequestMessage(
        HttpMethod.Get,
        "http://localhost:5002/api/data/protected");
    req2.Headers.Authorization =
        new System.Net.Http.Headers
            .AuthenticationHeaderValue("Bearer", token);
    var r2 = await http.SendAsync(req2);
    Console.WriteLine($"Result: HTTP {(int)r2.StatusCode}");
    Console.WriteLine(await r2.Content.ReadAsStringAsync());

    // 4. Me token të skaduar → 401
    Console.WriteLine(
        "\n[JWT] 4. Token i skaduar → do skadojë pas 30 min");
    Console.WriteLine(
        "Për demonstrim: ndrysho expires në 1 sekondë " +
        "në JwtService.cs dhe testo sërish");
}

// ── Helper ─────────────────────────────────────────
async Task<(int status, string body)> Post(
    string url, object data)
{
    try
    {
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(
            json, Encoding.UTF8, "application/json");
        var res = await http.PostAsync(url, content);
        var body = await res.Content.ReadAsStringAsync();
        return ((int)res.StatusCode, body);
    }
    catch (Exception ex)
    {
        return (0, ex.Message);
    }
}