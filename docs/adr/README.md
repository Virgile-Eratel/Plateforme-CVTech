# Architecture Decision Records

Décisions structurantes du projet **Plateforme-CVTech**. Chaque ADR est court : contexte,
décision, pourquoi. On ne crée un ADR que lorsqu'une décision est à la fois **difficile à
inverser**, **surprenante sans contexte** et **issue d'un arbitrage** réel.

| # | Décision |
|---|---|
| [0001](0001-monolithe-modulaire.md) | Architecture en monolithe modulaire |
| [0002](0002-cinq-couches-par-module.md) | Découpage en 5 couches par module |
| [0003](0003-communication-inter-modules.md) | Communication inter-modules : contrats publics + bus in-memory |
| [0004](0004-contrat-permission-centralise.md) | `IVerificateurPermission` centralisé dans GestionIdentite |
| [0005](0005-persistance-azure-sql-schema-par-module.md) | EF Core sur Azure SQL, un schéma par module |
| [0006](0006-vertical-slices-mediatr.md) | Vertical slices avec MediatR |
| [0007](0007-front-blazor-webassembly.md) | Front-end en Blazor WebAssembly |
| [0008](0008-authentification-jwt-identity.md) | Authentification ASP.NET Identity + JWT |
| [0009](0009-notifications-in-app-signalr.md) | Notifications in-app via SignalR |
| [0010](0010-deploiement-azure-bicep-devops.md) | Déploiement Azure : Bicep + Azure DevOps |
