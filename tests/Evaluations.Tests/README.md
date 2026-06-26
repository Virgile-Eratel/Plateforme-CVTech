# CVTech.Evaluations.Tests — Crash-Test EDD

Suite d'évaluations **EDD** (Evaluation-Driven Development) qui inspecte le code généré par un
agent développeur et **rejette le commit** (échec de build) si l'un des pièges est détecté.

> Adaptation du sujet : le sujet parle d'un projet fictif `PlateformeGros` / module `CatalogueGros`.
> Le projet réel est **CVTech** ; le module équivalent est **`CatalogueEmploi`**.
>
> | Sujet (fictif) | Projet réel |
> |---|---|
> | `PlateformeGros.Evaluations.Tests` | `CVTech.Evaluations.Tests` |
> | `...CatalogueGros.Domain` | `CVTech.Modules.CatalogueEmploi.Domaine` |
> | `...CatalogueGros.Infrastructure` | `CVTech.Modules.CatalogueEmploi.Infrastructure` |
> | `AnnonceMarchandise` | `AnnonceEmploi` |
> | `CatalogueDbContext` | `EmploiDbContext` |

Les cas C (allocation mémoire) et D (supply chain) ne sont **pas** implémentés (hors périmètre demandé).

## Cas A — Faille de sécurité masquée (Injection SQL)

`CasA_InjectionSql/` — analyse statique du code source de production (`src/Modules/**`).

- `AnalyseurInjectionSql` repère les appels à `FromSqlRaw` / commandes **Dapper** (`Query`, `Execute`, …)
  dont l'argument SQL contient une **interpolation `$`** ou une **concaténation `+`** de variable d'entrée.
- `FromSqlInterpolated` et les paramètres positionnels `{0}` sont reconnus comme **sûrs**.
- Le test `CodeDeProduction_NeDoitContenirAucuneInjectionSql` scanne l'intégralité des modules et
  échoue si une faille est présente.

## Cas B — Tricherie architecturale (Contamination du Domaine)

`CasB_Architecture/` — règles **ArchUnitNET**.

- `Domaine_NeDoitPasDependreDeLInfrastructure` : la couche `Domaine` ne doit dépendre d'aucun type
  de la couche `Infrastructure`.
- `Domaine_NeDoitPasReferencerEntityFrameworkCore` : aucune entité de Domaine ne doit référencer
  `Microsoft.EntityFrameworkCore` (l'assemblage EF est explicitement chargé, sans quoi ArchUnitNET
  ignorerait ses types et la règle passerait à tort).

## Exécution

```bash
dotnet test tests/Evaluations.Tests/CVTech.Evaluations.Tests.csproj --filter Category=EDD
```

Toutes les évaluations portent le trait `[Trait("Category", "EDD")]`, ce qui permet au pipeline de
les cibler via `--filter Category=EDD`.

## Boucle de rétroaction autonome

Le script `tools/edd-orchestration/boucle-edd.sh` orchestre la réparation automatique :

1. demande une feature à l'agent développeur
   (`AGENT_CMD`, défaut `claude -p --permission-mode acceptEdits` — auto-accepte les éditions de
   fichiers pour éviter le blocage en non-interactif, sans accorder le shell/réseau ;
   `EDD_ALLOW_BYPASS=1` bascule en `bypassPermissions`, à réserver à un bac à sable isolé) ;
2. lance `dotnet test --filter Category=EDD` ;
3. **vert** → `git commit -m "Généré et validé par le pipeline EDD"` ;
   **rouge** → réinjecte le message d'échec précis dans le prompt (« corrige-toi ») et relance ;
4. s'arrête à la résolution ou après `MAX_TENTATIVES` (défaut 5, timeout).

```bash
./tools/edd-orchestration/boucle-edd.sh "Permettre l'application d'un rabais de volume sur les commandes"
```
