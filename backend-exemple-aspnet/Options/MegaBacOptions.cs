namespace MegaBac.Api.Options;

public class MegaBacOptions
{
    public const string SectionName = "MegaBac";

    public string SignatureSecret { get; set; } = "";
    public int SignatureWindowSeconds { get; set; } = 30;
    public int SignatureTolerance { get; set; } = 1;

    public string DonneesDir { get; set; } = "";

    public int RateLimitPermitLimit { get; set; } = 30;
    public int RateLimitWindowSeconds { get; set; } = 60;
}
