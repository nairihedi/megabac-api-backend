# Megabac API Backend

API backend pour l'application Megabac de révision du Bac.

## Structure

- `backend-exemple-php/` - Implémentation en PHP
- `backend-exemple-aspnet/` - Implémentation en ASP.NET Core

## Configuration

Chaque backend expose les endpoints :
- `GET /api/{niveau}/version` - Version du contenu pour un niveau
- `GET /api/{niveau}/matieres` - Liste des matières
- `GET /api/{niveau}/matieres/{id}/version` - Version pour une matière
- `GET /api/{niveau}/matieres/{id}/cours` - Liste des cours
- `GET /api/{niveau}/cours/{id}` - Détail d'un cours
- `GET /api/{niveau}/exercices/{id}` - Détail d'un exercice
- `GET /api/{niveau}/epreuves/{id}` - Détail d'une épreuve

## Sécurité

- Signature TOTP (RFC 6238) pour anti-scraping
- Rate limiting
- User-Agent verification

## Déploiement

### PHP
```bash
php -S localhost:8000
```

### ASP.NET Core
```bash
dotnet run
```

## Voir aussi

- [megabac](https://github.com/your-user/megabac) - Application mobile
- [megabac-data-server](https://github.com/your-user/megabac-data-server) - Données statiques
- [megabac-admin-tool](https://github.com/your-user/megabac-admin-tool) - Outil d'administration
