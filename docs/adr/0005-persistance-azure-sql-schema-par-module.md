---
status: accepted
---
# 0005 — Persistance EF Core sur Azure SQL, un schéma SQL par module

La persistance utilise **Entity Framework Core** sur **Azure SQL Database**. Chaque module
possède son propre `DbContext` et son propre **schéma SQL** (ex : `identite.`, `emploi.`,
`freelance.`, `actualite.`). Les migrations sont gérées par module.

**Pourquoi** : un schéma par module matérialise l'étanchéité jusque dans la base — aucun
module ne lit les tables d'un autre. Azure SQL est imposé par le contexte de déploiement.
Une seule base mutualisée garde le coût et la plomberie raisonnables pour un TP.

## Alternatives écartées
- **Une base par module** : surcoût Azure inutile à cette échelle.
- **Dapper** : EF Core suffit et accélère les migrations demandées au rendu.
