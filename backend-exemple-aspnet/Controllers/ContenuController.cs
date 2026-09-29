using System.Text.RegularExpressions;
using MegaBac.Api.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MegaBac.Api.Controllers;

[ApiController]
[Route("api/{niveau}")]
public class ContenuController : ControllerBase
{
    private static readonly Regex IdValidePattern = new("^[a-zA-Z0-9_-]+$", RegexOptions.Compiled);
    private static readonly Regex NiveauValidePattern = new("^[0-9]+$", RegexOptions.Compiled);

    private readonly MegaBacOptions _options;

    public ContenuController(IOptions<MegaBacOptions> options)
    {
        _options = options.Value;
    }

    [HttpGet("version")]
    public IActionResult Version(string niveau) => LireFichier(niveau, "version.json");

    [HttpGet("matieres")]
    public IActionResult Matieres(string niveau) => LireFichier(niveau, "matieres.json");

    [HttpGet("matieres/{matiereId}/version")]
    public IActionResult MatiereVersion(string niveau, string matiereId) =>
        EstIdValide(matiereId) ? LireFichier(niveau, Path.Combine("matieres", matiereId, "version.json")) : BadRequest();

    [HttpGet("matieres/{matiereId}/cours")]
    public IActionResult CoursListe(string niveau, string matiereId) =>
        EstIdValide(matiereId) ? LireFichier(niveau, Path.Combine("cours", $"{matiereId}.json")) : BadRequest();

    [HttpGet("cours/{coursId}")]
    public IActionResult CoursDetail(string niveau, string coursId) =>
        EstIdValide(coursId) ? LireFichier(niveau, Path.Combine("cours-detail", $"{coursId}.json")) : BadRequest();

    [HttpGet("matieres/{matiereId}/exercices")]
    public IActionResult ExercicesListe(string niveau, string matiereId) =>
        EstIdValide(matiereId) ? LireFichier(niveau, Path.Combine("exercices", $"{matiereId}.json")) : BadRequest();

    [HttpGet("exercices/{exerciceId}")]
    public IActionResult ExerciceDetail(string niveau, string exerciceId) =>
        EstIdValide(exerciceId) ? LireFichier(niveau, Path.Combine("exercices-detail", $"{exerciceId}.json")) : BadRequest();

    [HttpGet("matieres/{matiereId}/epreuves")]
    public IActionResult EpreuvesListe(string niveau, string matiereId) =>
        EstIdValide(matiereId) ? LireFichier(niveau, Path.Combine("epreuves", $"{matiereId}.json")) : BadRequest();

    [HttpGet("epreuves/{epreuveId}")]
    public IActionResult EpreuveDetail(string niveau, string epreuveId) =>
        EstIdValide(epreuveId) ? LireFichier(niveau, Path.Combine("epreuves-detail", $"{epreuveId}.json")) : BadRequest();

    private static bool EstIdValide(string id) => IdValidePattern.IsMatch(id);

    // Lit un fichier JSON du jeu de données, avec vérification anti-traversée
    // de répertoire (équivalent du realpath() + StartsWith côté PHP).
    private IActionResult LireFichier(string niveau, string cheminRelatif)
    {
        if (!NiveauValidePattern.IsMatch(niveau))
        {
            return BadRequest();
        }

        var racine = Path.GetFullPath(_options.DonneesDir);
        var chemin = Path.GetFullPath(Path.Combine(racine, niveau, cheminRelatif));

        if (!chemin.StartsWith(racine, StringComparison.Ordinal) || !System.IO.File.Exists(chemin))
        {
            return NotFound(new { message = "Contenu introuvable." });
        }

        return PhysicalFile(chemin, "application/json; charset=utf-8");
    }
}
