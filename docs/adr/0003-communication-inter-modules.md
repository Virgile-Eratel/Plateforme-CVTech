---
status: accepted
---
# 0003 — Communication inter-modules : contrats publics + bus in-memory

Les modules ne se référencent jamais en interne. Toute interaction passe soit par un
**contrat public** (interface exposée, ex : `IVerificateurPermission`, `IReferentielDomaines`),
soit par un **bus d'événements interne en mémoire** (ex : `AnnoncePubliee`,
`AppelOffrePublie` consommés par `ActualiteEtAbonnement`).

**Pourquoi** : c'est le cœur du critère « étanchéité des modules ». Le couplage par
événements permet à `ActualiteEtAbonnement` de réagir aux publications sans que
`CatalogueEmploi` ou `AppelOffreFreelance` ne le connaissent.

## Conséquences
- Aucun projet `Module.*` ne référence le projet interne d'un autre module.
- Un projet `*.Contracts` par module expose ce qui est consommable de l'extérieur.
