---
status: accepted
---
# 0007 — Front-end en Blazor WebAssembly

L'interface des trois parcours (Candidat, Entreprise, Administrateur) est développée en
**Blazor WebAssembly**, dans la même solution .NET que le back-end.

**Pourquoi** : un seul écosystème (.NET/C#), partage direct des DTOs/contrats avec l'API,
pas de seconde chaîne de build ni de pipeline JS distinct — ce qui simplifie nettement le
déploiement Azure, critère de DX noté. Le consigne « ne pas faire du full JS » est respectée.

## Alternatives écartées
- **React (SPA)** : ajoute un build et un déploiement séparés (Static Web App) pour un
  bénéfice nul à cette échelle.
