<?php
// ============================================================================
// securite.php — Protections anti-scraping (friction contre les outils
// d'aspiration génériques ; pas une protection cryptographique absolue).
// ============================================================================

require_once __DIR__ . '/config.php';

function megabac_reponse_erreur(int $code, string $message): void {
    http_response_code($code);
    header('Content-Type: application/json; charset=utf-8');
    echo json_encode(['message' => $message], JSON_UNESCAPED_UNICODE);
    exit;
}

// ---------------------------------------------------------------------------
// 1) Filtrage User-Agent — bloque les signatures évidentes d'outils
//    d'aspiration (wget, HTTrack, scripts génériques). Contournable en
//    changeant l'UA, mais élimine la grande majorité des scripts utilisés
//    "tels quels" sans configuration particulière.
// ---------------------------------------------------------------------------
function megabac_verifier_user_agent(): void {
    $ua = $_SERVER['HTTP_USER_AGENT'] ?? '';
    $bloques = ['curl', 'wget', 'httrack', 'python-requests', 'scrapy', 'libwww-perl', 'go-http-client', 'java/'];

    if ($ua === '') {
        megabac_reponse_erreur(403, 'Requête refusée.');
    }
    $uaLower = strtolower($ua);
    foreach ($bloques as $motif) {
        if (str_contains($uaLower, $motif)) {
            megabac_reponse_erreur(403, 'Requête refusée.');
        }
    }
}

// ---------------------------------------------------------------------------
// 2) Limitation de débit par IP — fenêtre glissante, stockage fichier.
//    Volontairement simple (pas besoin de SQL Server pour un compteur
//    éphémère) ; à remplacer par une table dédiée si tu préfères centraliser.
// ---------------------------------------------------------------------------
function megabac_verifier_limite_debit(): void {
    $ip = $_SERVER['REMOTE_ADDR'] ?? 'inconnu';
    $dir = RATE_LIMIT_STORAGE_DIR;
    if (!is_dir($dir)) {
        @mkdir($dir, 0700, true);
    }
    $fichier = $dir . '/' . preg_replace('/[^a-zA-Z0-9_.]/', '_', $ip) . '.json';

    $maintenant = time();
    $horodatages = [];

    if (is_file($fichier)) {
        $contenu = @file_get_contents($fichier);
        $horodatages = json_decode($contenu ?: '[]', true) ?: [];
    }

    // Ne garder que les horodatages encore dans la fenêtre glissante.
    $horodatages = array_values(array_filter(
        $horodatages,
        fn($t) => $t > $maintenant - RATE_LIMIT_FENETRE_SECONDES
    ));

    if (count($horodatages) >= RATE_LIMIT_MAX_REQUETES) {
        header('Retry-After: ' . RATE_LIMIT_FENETRE_SECONDES);
        megabac_reponse_erreur(429, 'Trop de requêtes, réessaie dans un instant.');
    }

    $horodatages[] = $maintenant;
    @file_put_contents($fichier, json_encode($horodatages), LOCK_EX);
}

// ---------------------------------------------------------------------------
// 3) Signature temporelle (TOTP applicatif, RFC 6238) — le client envoie
//    l'en-tête X-App-Sig = HMAC-SHA256(secret, chemin + ':' + fenêtre).
//    Un code capturé (ex: via un proxy réseau) devient inutilisable après
//    quelques dizaines de secondes.
// ---------------------------------------------------------------------------
function megabac_verifier_signature(string $chemin): void {
    $signatureRecue = $_SERVER['HTTP_X_APP_SIG'] ?? '';
    if ($signatureRecue === '') {
        megabac_reponse_erreur(403, 'Signature manquante.');
    }

    $maintenant = time();
    $fenetreCourante = intdiv($maintenant, APP_SIGNATURE_WINDOW);

    for ($delta = -APP_SIGNATURE_TOLERANCE; $delta <= APP_SIGNATURE_TOLERANCE; $delta++) {
        $fenetre = $fenetreCourante + $delta;
        $attendu = hash_hmac('sha256', $chemin . ':' . $fenetre, APP_SIGNATURE_SECRET);
        if (hash_equals($attendu, $signatureRecue)) {
            return; // signature valide sur cette fenêtre (comparaison en temps constant)
        }
    }

    megabac_reponse_erreur(403, 'Signature invalide ou expirée.');
}

// ---------------------------------------------------------------------------
// Applique les trois protections d'un coup. À appeler en tête de chaque route,
// dans cet ordre : le moins coûteux (UA) d'abord, la signature en dernier.
// ---------------------------------------------------------------------------
function megabac_proteger(string $chemin): void {
    megabac_verifier_user_agent();
    megabac_verifier_limite_debit();
    megabac_verifier_signature($chemin);
}
