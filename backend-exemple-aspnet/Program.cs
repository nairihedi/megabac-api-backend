using System.Threading.RateLimiting;
using MegaBac.Api.Middleware;
using MegaBac.Api.Options;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.Configure<MegaBacOptions>(builder.Configuration.GetSection(MegaBacOptions.SectionName));

// Limitation de débit par IP, fenêtre glissante — intégrée à ASP.NET Core
// depuis .NET 7, aucun package NuGet supplémentaire nécessaire.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "inconnu";
        var megaBacOptions = context.RequestServices.GetRequiredService<IOptions<MegaBacOptions>>().Value;

        return RateLimitPartition.GetSlidingWindowLimiter(ip, _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = megaBacOptions.RateLimitPermitLimit,
            Window = TimeSpan.FromSeconds(megaBacOptions.RateLimitWindowSeconds),
            SegmentsPerWindow = 4,
            QueueLimit = 0, // pas de file d'attente : on rejette tout de suite en 429
        });
    });
});

var app = builder.Build();

// Ordre volontaire : le moins coûteux (rate limiter, natif) en premier,
// User-Agent ensuite, signature en dernier (le plus coûteux en calcul).
app.UseRateLimiter();
app.UseMiddleware<UserAgentFilterMiddleware>();
app.UseMiddleware<SignatureVerificationMiddleware>();

app.MapControllers();

app.Run();
