#!/usr/bin/env bash
#
# Boucle de Rétroaction Autonome — Pipeline EDD (Evaluation-Driven Development)
# ----------------------------------------------------------------------------
# 1. Demande à un Agent Développeur (CLI) de coder une feature.
# 2. Lance la suite d'évaluations EDD : dotnet test --filter Category=EDD.
# 3. Si VERT  -> git commit automatique « Généré et validé par le pipeline EDD ».
#    Si ROUGE -> capture le message d'échec, le réinjecte dans le prompt de l'agent
#                (« corrige-toi ») et relance la boucle.
# 4. S'arrête à la résolution complète ou après MAX_TENTATIVES (timeout).
#
# Usage :
#   ./boucle-edd.sh "Permettre l'application d'un rabais de volume sur les commandes"
#
# Variables d'environnement :
#   AGENT_CMD       Commande de l'agent dev. Reçoit le prompt sur STDIN.
#                   Défaut : « claude -p --permission-mode acceptEdits »
#                   (Claude Code headless : auto-accepte les ÉDITIONS de fichiers — ce qui évite le
#                    blocage en non-interactif — mais continue de demander une autorisation pour les
#                    autres outils, p. ex. shell/réseau. C'est volontairement plus restreint que
#                    bypassPermissions, qui accorderait un accès total).
#   EDD_ALLOW_BYPASS  1 pour passer en « bypassPermissions » (l'agent contourne TOUTE vérification de
#                   permission : shell, réseau, écriture hors dépôt). À n'activer QUE dans un bac à sable
#                   isolé (conteneur, utilisateur dédié sans secrets montés). Désactivé par défaut.
#   MAX_TENTATIVES  Nombre maximal de cycles (défaut : 5).
#   FILTRE_EDD      Filtre xUnit des évaluations (défaut : Category=EDD).
#   AUTO_COMMIT     1 pour committer en cas de succès (défaut : 1).
#   TIMEOUT_AGENT   Délai max (secondes) par invocation de l'agent (défaut : 600). 0 = désactivé.
#                   Nécessite « timeout » ou « gtimeout » (brew install coreutils sur macOS).

set -uo pipefail

# --- Configuration -----------------------------------------------------------
RACINE="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
PROJET_TESTS="$RACINE/tests/Evaluations.Tests/CVTech.Evaluations.Tests.csproj"
FEATURE="${1:-Permettre l'application d'un rabais de volume sur les commandes}"

MAX_TENTATIVES="${MAX_TENTATIVES:-5}"
FILTRE_EDD="${FILTRE_EDD:-Category=EDD}"
AUTO_COMMIT="${AUTO_COMMIT:-1}"
TIMEOUT_AGENT="${TIMEOUT_AGENT:-600}"

# Mode de permission de l'agent : « acceptEdits » (sûr par défaut, éditions de fichiers uniquement)
# ou « bypassPermissions » sur opt-in explicite EDD_ALLOW_BYPASS=1 (accès total — bac à sable requis).
EDD_ALLOW_BYPASS="${EDD_ALLOW_BYPASS:-0}"
if [[ "$EDD_ALLOW_BYPASS" == "1" ]]; then
  MODE_PERMISSION="bypassPermissions"
else
  MODE_PERMISSION="acceptEdits"
fi
AGENT_CMD="${AGENT_CMD:-claude -p --permission-mode $MODE_PERMISSION}"

# Détecte une commande « timeout » disponible (gtimeout sur macOS via coreutils).
TIMEOUT_BIN=""
if command -v timeout >/dev/null 2>&1; then TIMEOUT_BIN="timeout"
elif command -v gtimeout >/dev/null 2>&1; then TIMEOUT_BIN="gtimeout"; fi

JOURNAL="$(mktemp -t edd-test-XXXXXX.log)"
trap 'rm -f "$JOURNAL"' EXIT

# --- Helpers -----------------------------------------------------------------
info()  { printf '\033[1;34m[EDD]\033[0m %s\n' "$*"; }
ok()    { printf '\033[1;32m[EDD]\033[0m %s\n' "$*"; }
erreur(){ printf '\033[1;31m[EDD]\033[0m %s\n' "$*" >&2; }

# Invoque l'agent développeur avec un prompt fourni sur STDIN.
# L'agent est censé modifier les fichiers du dépôt en place.
invoquer_agent() {
  local prompt="$1"
  if ! command -v "${AGENT_CMD%% *}" >/dev/null 2>&1; then
    erreur "Agent introuvable : « ${AGENT_CMD%% *} »."
    erreur "Définissez AGENT_CMD vers votre CLI d'agent (ex. AGENT_CMD='claude -p')."
    exit 127
  fi
  info "Invocation de l'agent : ${AGENT_CMD}"

  local rc=0
  if [[ -n "$TIMEOUT_BIN" && "$TIMEOUT_AGENT" != "0" ]]; then
    printf '%s\n' "$prompt" | "$TIMEOUT_BIN" "$TIMEOUT_AGENT" $AGENT_CMD || rc=$?
    if (( rc == 124 )); then
      erreur "L'agent a dépassé le délai de ${TIMEOUT_AGENT}s et a été interrompu."
    fi
  else
    [[ -z "$TIMEOUT_BIN" ]] && info "« timeout » indisponible : invocation sans garde-fou de délai."
    printf '%s\n' "$prompt" | $AGENT_CMD || rc=$?
  fi
  return $rc
}

# Lance les évaluations EDD. Retourne le code de sortie de dotnet test.
lancer_evaluations() {
  info "Exécution des évaluations : dotnet test --filter $FILTRE_EDD"
  dotnet test "$PROJET_TESTS" --filter "$FILTRE_EDD" --nologo >"$JOURNAL" 2>&1
  return $?
}

# Extrait du journal les lignes pertinentes pour réinjection dans le prompt.
resume_echec() {
  grep -E '\[FAIL\]|EDD Rejet|Error Message|does depend on|Failed:' "$JOURNAL" \
    | sed 's/^[[:space:]]*//' \
    | sort -u
}

# --- Prompts -----------------------------------------------------------------
prompt_initial() {
  cat <<EOF
Tu es l'Agent Développeur du projet CVTech (monolithe modulaire DDD .NET 10).
Implémente la feature suivante dans le module concerné, en respectant strictement
l'architecture DDD (Domaine POCO sans EF Core, requêtes SQL paramétrées) :

  « $FEATURE »

Le code sera immédiatement soumis à une suite d'évaluations EDD automatisées.
EOF
}

prompt_correction() {
  cat <<EOF
Ton code a ÉCHOUÉ aux évaluations EDD du pipeline. Corrige-toi IMMÉDIATEMENT
sans introduire de régression. Voici le message de plantage précis du framework
de test à corriger :

$(resume_echec)

Rappels des règles évaluées :
  - Cas A : aucune injection SQL (pas de \$ ni de + dans FromSqlRaw/Dapper ; utilise des requêtes paramétrées).
  - Cas B : la couche Domaine ne doit JAMAIS dépendre de l'Infrastructure ni d'Entity Framework Core.
EOF
}

# --- Boucle de rétroaction ---------------------------------------------------
info "Racine du dépôt : $RACINE"
info "Feature demandée : $FEATURE"
info "Tentatives maximum : $MAX_TENTATIVES"
if [[ "$EDD_ALLOW_BYPASS" == "1" ]]; then
  erreur "⚠️  EDD_ALLOW_BYPASS=1 : l'agent contourne TOUTES les permissions (shell/réseau/FS)."
  erreur "⚠️  À n'utiliser qu'en bac à sable isolé, sans secrets ni accès au dépôt principal."
fi

prompt="$(prompt_initial)"

for (( tentative = 1; tentative <= MAX_TENTATIVES; tentative++ )); do
  info "──────── Tentative $tentative / $MAX_TENTATIVES ────────"

  invoquer_agent "$prompt"

  if lancer_evaluations; then
    ok "Toutes les évaluations EDD sont au VERT."
    if [[ "$AUTO_COMMIT" == "1" ]]; then
      git -C "$RACINE" add -A
      git -C "$RACINE" commit -m "Généré et validé par le pipeline EDD" \
        && ok "Commit automatique effectué."
    else
      info "AUTO_COMMIT désactivé : aucun commit réalisé."
    fi
    exit 0
  fi

  erreur "Échec des évaluations EDD (tentative $tentative). Détails :"
  resume_echec | sed 's/^/    /' >&2

  if (( tentative < MAX_TENTATIVES )); then
    info "Réinjection du message d'échec dans le prompt de l'agent…"
    prompt="$(prompt_correction)"
  fi
done

erreur "TIMEOUT : $MAX_TENTATIVES tentatives épuisées sans résolution. Intervention humaine requise."
exit 1
