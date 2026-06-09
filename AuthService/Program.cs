using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using AuthService.Data;
using AuthService.Hubs;
using AuthService.Services;

var builder = WebApplication.CreateBuilder(args);

var secret = builder.Configuration["Jwt:Secret"]
    ?? "SuperSecretKey_MinLength32Chars!!";

builder.Services.AddAuthentication(
    JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "JwtAiSystem",
                ValidAudience = "JwtAiSystem",
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secret))
            };
    });

builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddDbContext<AuthDbContext>(o =>
    o.UseSqlite("Data Source=auth.db"));

builder.Services.AddSingleton<BlockingService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<AiClientService>();

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<AuthDbContext>();
    db.Database.EnsureCreated();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();
app.MapHub<DashboardHub>("/dashboardHub");
app.MapFallbackToFile("index.html");

app.Run("http://localhost:5001");