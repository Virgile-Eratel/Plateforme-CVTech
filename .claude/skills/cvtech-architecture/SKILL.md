---
name: cvtech-architecture
description: Règles d'architecture du monolithe modulaire Plateforme-CVTech (.NET 10). À consulter AVANT d'ajouter ou modifier du code sous src/Modules/**, src/SharedKernel ou src/Api : structure des 5 couches d'un module, sens des dépendances, interdictions strictes (pas de DbContext dans le Domaine, pas de logique dans les endpoints, pas de référence croisée entre modules), communication inter-modules (contrat public ou bus d'événements) et vertical slices.
---

# CONTEXTE
Nous sommes en 2026. Le projet **Plateforme-CVTech** est un **monolithe modulaire** en
**.NET 10**. Le code doit refléter le langage métier en **français** pour les couches
Domaine et Application, tout en gardant les standards industriels (**anglais**) pour
l'infrastructure et la technique (Controllers, Handlers, Infrastructure, ModuleLoader).

Avant toute génération, consulte :
- `CONTEXT.md` — glossaire du langage métier (orthographe et sens des termes à respecter).
- `docs/adr/0001` à `0003`, `0006` — décisions d'architecture.

S'applique à : `src/Modules/**/*.cs` (et par extension `src/SharedKernel`, `src/Api`).

# INSTRUCTIONS

## 1. Les 5 couches d'un module (ADR 0002) — sens des dépendances
Pour `src/Modules/<Module>/` la structure est **imposée** :
```
Client/          Minimal APIs (endpoints), DTOs requête/réponse, mappings. SEULE porte d'entrée.
Application/      Features/<CasDUsage>/ : Command|Query + Handler + Validator (vertical slice).
Domaine/          Entités riches, Agrégats, Value Objects, Exceptions métier. 100 % FR.
Infrastructure/   EF Core (DbContext, configurations, migrations), services externes, RSS.
<Module>Loader.cs Composition root : enregistrement DI du module.
```
Règle de dépendance (vers l'intérieur uniquement) :
`Client → Application → Domaine` ; `Infrastructure → Domaine` (implémente ses ports).
Le **Domaine ne dépend de RIEN** de technique.

## 2. Interdictions strictes (refuse de générer du code qui les viole)
- ❌ **Aucun `DbContext`, `DbSet`, attribut EF Core ou using `Microsoft.EntityFrameworkCore`
  dans `Domaine/`.** La persistance vit exclusivement dans `Infrastructure/`.
- ❌ **Aucune logique métier dans un Controller / endpoint Client.** Un endpoint se contente
  de mapper un DTO → Command/Query, d'envoyer via MediatR, et de mapper le résultat → DTO.
- ❌ **Aucune référence de projet entre deux modules.** `Modules/A` ne référence jamais le
  projet interne de `Modules/B`. (cf. §3)
- ❌ **Aucun `using` croisé** entre les dossiers `Domaine`/`Application`/`Infrastructure` de
  deux modules différents.
- ❌ Pas d'entité « anémique » : la logique d'invariant vit dans l'entité/agrégat du Domaine,
  pas dans le Handler.
- ❌ Pas d'accès direct, depuis un module, à la base de données d'un autre module.

## 3. Communication inter-modules (ADR 0003)
Deux moyens **autorisés et exclusifs** :
1. **Contrat public** : une interface exposée dans un projet `<Module>.Contracts`
   (ex : `IVerificateurPermission`, `IReferentielDomaines`), injectée via le `ModuleLoader`.
2. **Bus d'événements interne en mémoire** : publication d'un événement
   (ex : `AnnoncePubliee`, `AppelOffrePublie`) consommé par un autre module via un handler.
   L'émetteur **ne connaît pas** ses consommateurs.
Quand on te demande une interaction entre modules, propose toujours l'une de ces deux voies,
jamais un appel direct.

## 4. Vertical slice (ADR 0006)
Une fonctionnalité = un dossier `Application/Features/<NomCasDUsageFR>/` contenant :
- `<CasDUsage>Command.cs` ou `<CasDUsage>Query.cs` (record),
- `<CasDUsage>Handler.cs` (implémente `IRequestHandler<...>` MediatR),
- `<CasDUsage>Validator.cs` (FluentValidation).
Nommer les cas d'usage en **français** (ex : `PostulerAnnonce`, `PublierAppelOffre`).

## 5. Ubiquitous Language
Tout type du Domaine et de l'Application porte un nom **français** issu de `CONTEXT.md`
(`AnnonceEmploi`, `CahierDesCharges`, `PropositionFreelance`, `Abonnement`…). La plomberie
(`...Handler`, `...Controller`, `...DbContext`, `ModuleLoader`) reste en anglais.

## 6. Quand tu ajoutes une fonctionnalité
1. Place-la dans le bon module et la bonne couche.
2. Crée la vertical slice complète (Command/Query + Handler + Validator).
3. Si elle touche un autre module → événement ou contrat public, jamais de référence directe.
4. Respecte d'abord la skill `cvtech-permissions` (vérif en tête de handler) et `cvtech-tdd`
   (test rouge d'abord).
