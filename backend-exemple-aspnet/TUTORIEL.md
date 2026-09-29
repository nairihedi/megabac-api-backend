# Backend ASP.NET Core — création du projet dans Visual Studio

Ce dossier ne contient que les fichiers "métier" (contrôleur, middlewares,
options) à copier dans un projet que tu crées toi-même. Voici les points
spécifiques à ne pas manquer.

## 1. Créer le projet

Visual Studio → *Créer un nouveau projet* → **ASP.NET Core Web API**.

- Nom : `MegaBac.Api`
- Framework cible : **.NET 8 (LTS)**
- Type d'authentification : **Aucune**
- Configurer pour HTTPS : oui
- Activer OpenAPI (Swagger) : facultatif, pas nécessaire pour ce projet
- **Utiliser des contrôleurs** : coché (pas les API minimales) — les fichiers
  fournis sont écrits pour des contrôleurs classiques.

## 2. Nettoyer le scaffolding par défaut

Supprimer les fichiers générés par le template que tu n'utiliseras pas :
`Controllers/WeatherForecastController.cs` et `WeatherForecast.cs` à la
racine.

## 3. Copier les fichiers fournis

Copier tel quel dans le projet, en respectant les dossiers :
- `Options/MegaBacOptions.cs`
- `Middleware/UserAgentFilterMiddleware.cs`
- `Middleware/SignatureVerificationMiddleware.cs`
- `Controllers/ContenuController.cs`

Remplacer le `Program.cs` généré par celui fourni ici (ou fusionner
manuellement si tu as déjà des personnalisations dedans).

Le tout utilise des espaces de noms `MegaBac.Api.*` — comme le SDK-style
project + *implicit usings* d'ASP.NET Core déduisent l'espace de noms du
dossier automatiquement, ça devrait fonctionner sans rien changer si le
projet s'appelle bien `MegaBac.Api`. Sinon, ajuste les `namespace` en haut de
chaque fichier pour qu'ils correspondent au nom réel de ton projet.

## 4. Configuration

Ouvrir `appsettings.json` généré par Visual Studio et fusionner dedans le
contenu de `appsettings.MegaBac.snippet.json` fourni ici (une section
`"MegaBac": { … }` au même niveau que `"Logging"`).

Deux valeurs à adapter obligatoirement :
- **`SignatureSecret`** : une chaîne aléatoire longue, **strictement
  identique** à `APP_SIGNATURE_SECRET` dans `www/js/api.js` côté app.
- **`DonneesDir`** : chemin absolu vers `exemple-donnees-serveur/` sur le
  serveur (ou vers tes vraies données une fois le contenu géré autrement).

## 5. Tester en local (F5)

Le rate limiter et le filtrage User-Agent ne posent pas de souci pour un
test au navigateur, mais la **vérification de signature bloquera tout appel
qui n'en fournit pas** — un simple `https://localhost:xxxx/api/6/matieres`
tapé dans le navigateur renverra donc un 403, ce qui est normal : seul le
vrai client (l'app, qui calcule la signature) doit passer. Pour un test
manuel rapide, deux options :
- commenter temporairement `app.UseMiddleware<SignatureVerificationMiddleware>();`
  dans `Program.cs`, le temps du test, puis la remettre ;
- ou générer une signature valide à la main avec un script court (même
  algorithme que `calculerSignature()` dans `www/js/api.js` : HMAC-SHA256 du
  chemin + `:` + `timestamp_unix / 30`, en hexadécimal).

## 6. Déployer sur IIS

Comme pour tes apps ASP.NET Framework existantes, mais avec une pièce en
plus propre à ASP.NET **Core** :

1. Installer sur le serveur le **.NET 8 Hosting Bundle** (pas seulement le
   runtime .NET — le Hosting Bundle inclut le module IIS `AspNetCoreModuleV2`
   nécessaire pour qu'IIS sache faire tourner une app Core). Téléchargeable
   depuis la page officielle *.NET Downloads* de Microsoft. Redémarrer IIS
   (`iisreset`) après installation.
2. Dans IIS Manager, créer un site (ou une application) pointant vers le
   dossier de publication.
3. Le pool d'applications doit être configuré en **"No Managed Code"**
   (ASP.NET Core gère son propre runtime, contrairement à ASP.NET Framework
   qui a besoin du pool en mode géré .NET).
4. Publier depuis Visual Studio (clic droit sur le projet → *Publier* →
   *Dossier*, ou *Serveur IIS* si tu configures une connexion directe) — le
   `web.config` nécessaire au module IIS est généré automatiquement à la
   publication, pas besoin de l'écrire à la main.
