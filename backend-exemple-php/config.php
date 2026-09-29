<?php
// ============================================================================
// config.php — Réglages du backend d'exemple MegaBac.
// ============================================================================

// Secret partagé avec l'application (voir APP_SIGNATURE_SECRET dans
// www/js/api.js). DOIT être identique des deux côtés.
//
// ⚠️ Pas un secret cryptographique fort : présent en clair dans le JS livré
// au téléphone, extractible par quelqu'un qui décompile l'app. Sert à
// dissuader les outils de scraping génériques, pas à bloquer un attaquant
// ciblant spécifiquement cette app.
define('APP_SIGNATURE_SECRET', 'CHANGE-MOI-avec-une-chaine-aleatoire-longue');

// Fenêtre de temps du TOTP applicatif, en secondes (doit correspondre à
// APP_SIGNATURE_WINDOW_SECONDS côté client).
define('APP_SIGNATURE_WINDOW', 30);
// Tolérance : nombre de fenêtres avant/après acceptées, pour absorber un
// décalage d'horloge du téléphone.
define('APP_SIGNATURE_TOLERANCE', 1);

// Limitation de débit par IP (fenêtre glissante).
define('RATE_LIMIT_MAX_REQUETES', 30);      // requêtes...
define('RATE_LIMIT_FENETRE_SECONDES', 60);  // ...par fenêtre glissante de 60 secondes
define('RATE_LIMIT_STORAGE_DIR', __DIR__ . '/../cache-rate-limit');

// Chemin vers le jeu de données JSON statique (voir exemple-donnees-serveur/).
define('DONNEES_DIR', __DIR__ . '/../exemple-donnees-serveur');
