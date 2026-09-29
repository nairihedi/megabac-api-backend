<?php
// ============================================================================
// index.php — Contrôleur frontal du backend d'exemple MegaBac.
//
// Sert le jeu de données JSON statique (exemple-donnees-serveur/) via les
// routes du contrat d'API, après application des 3 protections anti-scraping
// (voir securite.php). Un vrai backend remplacerait la lecture de fichiers
// par des requêtes SQL Server, mais la logique de routage et de sécurité
// reste la même.
// ============================================================================

require_once __DIR__ . '/securite.php';

$chemin = parse_url($_SERVER['REQUEST_URI'], PHP_URL_PATH);

if (!preg_match('#^/api/(?<niveau>[0-9]+)/(?<reste>.+)$#', $chemin, $m)) {
    megabac_reponse_erreur(404, 'Route inconnue.');
}

// Protections appliquées avant toute lecture de fichier.
megabac_proteger($chemin);

$niveau = $m['niveau'];
$reste = $m['reste'];
$base = DONNEES_DIR . '/' . $niveau;

$fichier = null;
$idPattern = '[a-zA-Z0-9_-]+';

if ($reste === 'version') {
    $fichier = "$base/version.json";
} elseif ($reste === 'matieres') {
    $fichier = "$base/matieres.json";
} elseif (preg_match("#^matieres/(?<id>$idPattern)/version$#", $reste, $mm)) {
    $fichier = "$base/matieres/{$mm['id']}/version.json";
} elseif (preg_match("#^matieres/(?<id>$idPattern)/cours$#", $reste, $mm)) {
    $fichier = "$base/cours/{$mm['id']}.json";
} elseif (preg_match("#^cours/(?<id>$idPattern)$#", $reste, $mm)) {
    $fichier = "$base/cours-detail/{$mm['id']}.json";
} elseif (preg_match("#^matieres/(?<id>$idPattern)/exercices$#", $reste, $mm)) {
    $fichier = "$base/exercices/{$mm['id']}.json";
} elseif (preg_match("#^exercices/(?<id>$idPattern)$#", $reste, $mm)) {
    $fichier = "$base/exercices-detail/{$mm['id']}.json";
} elseif (preg_match("#^matieres/(?<id>$idPattern)/epreuves$#", $reste, $mm)) {
    $fichier = "$base/epreuves/{$mm['id']}.json";
} elseif (preg_match("#^epreuves/(?<id>$idPattern)$#", $reste, $mm)) {
    $fichier = "$base/epreuves-detail/{$mm['id']}.json";
}

// realpath() + vérification du préfixe : empêche toute tentative de
// traversée de répertoire (ex: id contenant "../..") de sortir de DONNEES_DIR.
$fichierReel = $fichier ? realpath($fichier) : false;
$baseReelle = realpath(DONNEES_DIR);

if (!$fichierReel || !$baseReelle || !str_starts_with($fichierReel, $baseReelle) || !is_file($fichierReel)) {
    megabac_reponse_erreur(404, 'Contenu introuvable.');
}

header('Content-Type: application/json; charset=utf-8');
readfile($fichierReel);
