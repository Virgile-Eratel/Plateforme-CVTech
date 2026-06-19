---
name: cvtech-relecture-backend-code
description: Skill de relecture backend pour Plateforme-CVTech. Vérifie les patterns d'architecture modules, l'organisation vertical slice, les cas d'usage, la séparation client/application/domaine/infrastructure, la conformité CQRS/DD, et liste brièvement les manques ou améliorations.
---

# CONTEXTE
Le projet **Plateforme-CVTech** est un monolithe modulaire en **.NET 10**. Ce skill sert à effectuer une relecture backend ciblée sur les modules métier, en respectant les ADR et les conventions du code.

À consulter avant toute analyse :
- `CONTEXT.md`
- `docs/adr/0001-monolithe-modulaire.md`
- `docs/adr/0002-cinq-couches-par-module.md`
- `docs/adr/0003-communication-inter-modules.md`
- `docs/adr/0004-contrat-permission-centralise.md`
- `docs/adr/0006-vertical-slices-mediatr.md`

S'applique prioritairement à :
- `src/Modules/**/*.cs`
- `src/SharedKernel/**/*.cs`
- `src/Api/**/*.cs`

# INSTRUCTIONS

## 1. Objectif principal
Donne une relecture concise du backend :
- ce qui est correct,
- ce qui manque,
- ce qui doit être amélioré.

Répond toujours en bref, par points courts.

## 2. Structure de la réponse
Pour chaque module ou dossier analysé, sépare les commentaires en briques :
- **Module.[Feature].Domaine** : entités, agrégats, value objects, logique métier.
- **Module.[Feature].Application** : handlers, validators, **et endpoints** (vertical slices, Features).
- **Module.[Feature].Client** : contrats publics (DTOs, énums). Packagé en NuGet, pas d'endpoints HTTP ici.
- **Module.[Feature].Infrastructure** : persistance EF Core, services externes, dépôts.
- **Module.[Feature].Loader** : composition root, injection de dépendances.

Ajoute une phrase finale courte avec la priorité d'amélioration la plus importante.

## 3. Vrai / faux positif
Ne signale pas comme erreur :
- l'utilisation de MediatR pour un handler.
- la création de `Command`/`Query` dans `Application/Features`.
- l'emploi de noms métier français dans le domaine.
- la présence de **deux classes distinctes** pour une même entité logique :
  - `AnnonceEmploi` (Domaine) : agrégat riche avec logique métier.
  - `AnnonceEntity` (Infrastructure) : entité EF Core plate, simple DAO.
  - C'est du **bon DDD**, pas de la duplication.

## 4. Ce qu'il faut détecter

### Domaine
- vérifier qu'il n'y a pas de `DbContext`, `DbSet`, ou `Microsoft.EntityFrameworkCore` dans `Domaine/`.
- vérifier que la logique métier et les invariants sont dans les entités/agrégats, pas dans les handlers.
- remarquer si des exceptions métier explicites manquent et qu'on utilise trop `InvalidOperationException`.

### Application / Features (Endpoints)
- repérer si un cas d'usage est bien isolé en vertical slice (`Features/<NomCasDUsage>/`).
- vérifier la présence d'un `Handler`, `Validator` et d'un `Command` ou `Query`.
- repérer l'absence de vérification de permissions en tête de handler.
- repérer les handlers qui manipulent directement la persistance ou des services externes sans passer par un port.
- noter si les interfaces de dépôt sont trop minimales pour les besoins métiers recensés.
- **Endpoints minimaux** : dans `Application/Features/<NomCasDUsage>/`, vérifier qu'il existe un fichier `*Endpoint.cs` qui mappe la route HTTP via `MapGet/MapPost/etc.`, reçoit les DTOs du Client, et délègue à MediatR (l'endpoint ne contient **aucune logique métier**).
- Vérifier que chaque cas d'usage a son endpoint dans Application.

### Client (Contrats / Package NuGet)
- **Rôle** : le Client est exporté en tant que **package NuGet** (contrat public consommable par d'autres systèmes).
- **Contient** : DTOs (`*Requete`, `*Reponse`), énums métier, interfaces publiques.
- **NE contient PAS** : endpoints HTTP, logique métier, détails d'infrastructure.
- Vérifier que tous les DTOs pour une feature sont bien dans `Client/` pour être packagés.
- Vérifier l'absence de `MapGet`, `MapPost`, etc. — ces routes vivent dans `Application/Features`.

### Infrastructure / Repositories
- **Nommage** : les classes d'accès à la BDD doivent utiliser le suffixe `*Repository` (ex : `AnnonceRepository`, `PropositionRepository`), jamais `Depot*EfCore`.
- **Injection du contexte** : le DbContext doit être injecté directement dans le constructeur du repository comme dépendance.
  ```csharp
  public sealed class AnnonceRepository(EmploiDbContext contexte) : IDepotAnnonces
  ```
- **Utilisation directe de SaveChanges** : appeler `contexte.SaveChangesAsync()` directement dans les méthodes (pas d'intermédiaire `EnregistrerAsync()` artificiel).
  ```csharp
  public async Task AjouterAsync(AnnonceEmploi annonce, CancellationToken ct)
  {
      var entite = AnnonceMapper.ToEntity(annonce);
      contexte.Annonces.Add(entite);
      await contexte.SaveChangesAsync(ct);
  }
  ```
- **Séparation Domaine / Infrastructure** :
  - **Domaine** : `AnnonceEmploi` (objet métier riche, comportement).
  - **Infrastructure** : `AnnonceEntity` (ou `AnnonceEf`) — entité de BDD plate, sans logique.
  - Jamais d'utilisation directe d'`AnnonceEntity` dans l'Application ou le Domaine.
- **Mappers dédiés** : créer une classe `*Mapper` par repository (ex : `AnnonceMapper`, `PropositionMapper`) contenant les conversions Entity ↔ Domain.
  ```csharp
  public static class AnnonceMapper
  {
      public static AnnonceEntity ToEntity(AnnonceEmploi domaine) { ... }
      public static AnnonceEmploi ToDomain(AnnonceEntity entite) { ... }
  }
  ```
- **Pas de complexité artificielle** : éviter les couches intermédiaires (pas d'`IUnitOfWork`, `IRepository<T>` générique inutile). Appels directs et simples.
- **Interface de port** : le port (`IDepotAnnonces`) doit rester dans `Application/` ou `Contracts/`, sans détails EF Core.
- Signaler quand une interface de dépôt manque des requêtes attendues (`ListerPar...`, `ObtenirPar...`, etc.).

### Loader / Composition Root
- Vérifier le `ModuleLoader` (ex : `ModuleLoader.cs`) : c'est la composition root du module.
- Rôle : enregistrer l'injection de dépendances (implémentations des ports), configurer MediatR, et enregistrer le `DbContext` et les services infra.
- Règles :
	- Ne contient **aucune logique métier**.
	- N'effectue **pas** d'appels directs vers d'autres modules (utiliser contrats publics ou événements).
	- Ne doit pas exposer de détails EF Core vers la couche Application.
- Exemples d'enregistrements acceptés :
	- `services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));`
	- `services.AddDbContext<ModuleDbContext>(configurerBdd);`
	- `services.AddScoped<IDepotX, DepotXEfCore>();`
- Vérifier que les contrats (`Contracts`) sont mappés vers leurs implémentations ici, et non l'inverse.

## 5. Comment formuler la relecture
- pour chaque point, écrire en français court.
- privilégier le format : `- Domaine : ...`, `- Application : ...`.
- éviter les paragraphes longs.
- finir par une synthèse type : `Priorité : ...`.

## 6. Exemples de formulation
- `Domaine : OK, invariants bien encapsulés.`
- `Application : manque un validator pour ce command.`
- `Application/Endpoint : manque *Endpoint.cs pour mapper les routes HTTP.`
- `Client : DTOs bien isolés dans Client/, contrat NuGet prêt.`
- `Infrastructure/Repository : classe doit s'appeler AnnonceRepository (pas DepotAnnoncesEfCore), contexte injecté direct, SaveChangesAsync() appelé directement.`
- `Infrastructure/Mapper : manque AnnonceMapper pour mapper Entity ↔ Domain.`
- `Infrastructure/Entity : AnnonceEntity bien isolée du Domaine.`
- `Loader : ...`

# USAGE
Quand on te demande une `relecture backend`, `audit code`, `review code`, ou `analyse module`, applique ce skill en priorité pour générer un bilan structuré et bref.
