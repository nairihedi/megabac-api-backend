using System.Security.Cryptography;
using System.Text;
using MegaBac.Api.Options;
using Microsoft.Extensions.Options;

namespace MegaBac.Api.Middleware;

// Signature temporelle (type TOTP, RFC 6238) : le client envoie
// X-App-Sig = HMAC-SHA256(secret, chemin + ':' + fenêtre_de_30s).
// Un code capturé devient inutilisable après quelques dizaines de secondes.
//
// ⚠️ Pas un secret cryptographique fort de bout en bout : il est présent en
// clair dans le JS embarqué côté client. Sert à dissuader les outils de
// scraping génériques, pas à bloquer un attaquant ciblant spécifiquement
// cette app.
public class SignatureVerificationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly MegaBacOptions _options;

    public SignatureVerificationMiddleware(RequestDelegate next, IOptions<MegaBacOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var signatureRecue = context.Request.Headers["X-App-Sig"].ToString();

        if (string.IsNullOrEmpty(signatureRecue) || !EstSignatureValide(context.Request.Path, signatureRecue))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { message = "Signature invalide ou expirée." });
            return;
        }

        await _next(context);
    }

    private bool EstSignatureValide(PathString chemin, string signatureRecue)
    {
        byte[] octetsRecus;
        try
        {
            octetsRecus = Convert.FromHexString(signatureRecue);
        }
        catch (FormatException)
        {
            return false; // en-tête malformé, ni une signature ni une attaque à investiguer
        }

        var maintenant = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var fenetreCourante = maintenant / _options.SignatureWindowSeconds;

        for (var delta = -_options.SignatureTolerance; delta <= _options.SignatureTolerance; delta++)
        {
            var fenetre = fenetreCourante + delta;
            var message = $"{chemin}:{fenetre}";
            var attendu = CalculerHmac(message);

            if (octetsRecus.Length == attendu.Length && CryptographicOperations.FixedTimeEquals(octetsRecus, attendu))
            {
                return true; // comparaison en temps constant
            }
        }

        return false;
    }

    private byte[] CalculerHmac(string message)
    {
        var cle = Encoding.UTF8.GetBytes(_options.SignatureSecret);
        using var hmac = new HMACSHA256(cle);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
    }
}
