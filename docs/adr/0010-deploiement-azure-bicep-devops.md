---
status: accepted
---
# 0010 — Déploiement Azure : Bicep + Azure DevOps Pipelines

L'infrastructure est décrite en **Bicep** (Azure SQL Database + App Service hébergeant
l'API et le Blazor WASM). Le déploiement est automatisé par un pipeline **Azure DevOps**
(`azure-pipelines.yml`) exécuté sur l'agent self-hosted déjà disponible.

**Pourquoi** : Bicep est l'IaC native Azure, reproductible et versionnée (DX notée). Azure
DevOps est cohérent avec l'agent `vsts-agent` présent dans l'environnement de l'étudiant.

## Conséquences
- `restore → test → build → publish → deploy` ; le déploiement n'a lieu que si les tests
  sont au vert, conformément au diagramme Mermaid du rendu.
- Les secrets (chaîne Azure SQL, signing key JWT) passent par des variables de pipeline.
