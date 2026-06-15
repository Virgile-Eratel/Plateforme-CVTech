---
status: accepted
---
# 0008 — Authentification par ASP.NET Core Identity + JWT

L'authentification s'appuie sur **ASP.NET Core Identity** (stockage des comptes dans le
schéma `identite`) et délivre des **jetons JWT** consommés par le front Blazor WASM et par
le pipeline d'autorisation. Les rôles portés par le jeton sont `Candidat`, `Entreprise`,
`Administrateur`.

**Pourquoi** : les annonces et appels d'offre sont publics (consultation anonyme), mais
toute action (CV, candidature, abonnement, publication) exige une identité. JWT est sans
état, simple à propager vers SignalR et adapté à un front WASM.

## Conséquences
- L'endpoint RSS reste **anonyme** (aucun jeton requis).
- Le jeton alimente `IVerificateurPermission` (qui est l'appelant, quel rôle).
