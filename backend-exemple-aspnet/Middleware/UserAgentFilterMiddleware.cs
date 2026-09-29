namespace MegaBac.Api.Middleware;

// Bloque les signatures évidentes d'outils d'aspiration (wget, HTTrack,
// scripts génériques). Contournable en changeant l'User-Agent, mais élimine
// la grande majorité des outils utilisés "tels quels" sans configuration.
public class UserAgentFilterMiddleware
{
    private static readonly string[] MotifsBloques =
    {
        "curl", "wget", "httrack", "python-requests", "scrapy", "libwww-perl", "go-http-client", "java/"
    };

    private readonly RequestDelegate _next;

    public UserAgentFilterMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var userAgent = context.Request.Headers.UserAgent.ToString();

        var bloque = string.IsNullOrWhiteSpace(userAgent)
            || MotifsBloques.Any(m => userAgent.Contains(m, StringComparison.OrdinalIgnoreCase));

        if (bloque)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { message = "Requête refusée." });
            return;
        }

        await _next(context);
    }
}
