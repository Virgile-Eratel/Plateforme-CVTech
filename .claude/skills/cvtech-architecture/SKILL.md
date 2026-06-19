---
name: cvtech-architecture
description: Règles d'architecture du monolithe modulaire Plateforme-CVTech (.NET 10), alignées sur la conception « microservice orienté DDD » de Microsoft (Domaine POCO → Application → Infrastructure). À consulter AVANT d'ajouter ou modifier du code sous src/Modules/**, src/SharedKernel ou src/Api : structure d'un module aplati (Contracts + Domaine/Application/Infrastructure/Client), couche Domaine organisée en agrégats (AggregatesModel) avec interfaces de dépôt, sens des dépendances, interdictions strictes (pas d'EF Core dans le Domaine, pas de logique dans les endpoints, pas de référence croisée entre modules), communication inter-modules (Contracts public ou bus d'événements), Client en ports entrée/sortie et vertical slices.
---

# CONTEXTE
Nous sommes en 2026. Le projet **Plateforme-CVTech** est un **monolithe modulaire** en
**.NET 10**. Chaque module est un **contexte délimité** (bounded context) au sens DDD et suit
la conception en couches du guide Microsoft *« Concevoir un microservice orienté DDD »*
(Domaine au cœur, Application mince, Infrastructure en périphérie).

Le code doit refléter le langage métier en **français** pour les couches Domaine et
Application, tout en gardant les standards industriels (**anglais**) pour l'infrastructure et
la technique (Handlers, DbContext, Configurations, ModuleLoader).

Avant toute génération, consulte :
- `CONTEXT.md` — glossaire du langage métier (orthographe et sens des termes à respecter).
- `docs/adr/0001` à `0003`, `0006` — décisions d'architecture.

S'applique à : `src/Modules/**/*.cs` (et par extension `src/SharedKernel`, `src/Api`).

# INSTRUCTIONS

## 1. Structure d'un module (ADR 0002) — sens des dépendances
Un module = **un projet public `Contracts` + un projet interne** posé directement sous
`src/Modules/<Module>/` (PAS de dossier `Module/` intermédiaire). La structure est **imposée** :
```
src/Modules/<Module>/
  Contracts/            Projet PUBLIC séparé (CVTech.Modules.<Module>.Contracts) :
                        événements d'intégration + interfaces publiques. SEUL projet
                        qu'un autre module a le droit de référencer (cf. §3).
  Domaine/              Cœur métier. POCO 100 % FR, AUCUNE dépendance technique. Voir §4.
  Application/          Orchestration mince. Features/<CasDUsage>/ (vertical slice). Dépend du Domaine.
  Infrastructure/       EF Core (DbContext, Configurations, Migrations) + implémentation des
                        dépôts (ports du Domaine) + services externes (RSS…). Dépend du Domaine.
  Client/               Adaptateur HTTP = seule porte d'entrée réseau. Ports + wiring. Voir §5.
  <Module>Loader.cs     Composition root : enregistrement DI du module.
  CVTech.Modules.<Module>.csproj
```
Règle de dépendance (les flèches pointent vers l'intérieur, le Domaine au centre) :
`Client → Application → Domaine` ; `Infrastructure → Domaine` (implémente ses ports).
Le **Domaine ne dépend de RIEN** de technique (ni EF Core, ni ASP.NET, ni MediatR).
Les classes de base du Domaine (SeedWork) — `Entite`, `ObjetValeur`, `RacineAgregat<TId>` —
vivent dans `SharedKernel/Domaine` et sont **réutilisées** : on ne les redéclare pas par module.

## 2. Interdictions strictes (refuse de générer du code qui les viole)
- ❌ **Aucun `DbContext`, `DbSet`, attribut EF Core ou using `Microsoft.EntityFrameworkCore`
  dans `Domaine/`.** La persistance vit exclusivement dans `Infrastructure/`.
- ❌ **Aucune logique métier dans un endpoint.** Un endpoint mappe un DTO → Command/Query,
  envoie via MediatR, et mappe le résultat → réponse. Rien d'autre.
- ❌ **Aucune référence de projet vers le projet interne d'un autre module.** `Modules/A` ne
  référence QUE le projet `Modules/B/Contracts` de B, jamais son projet interne. (cf. §3)
- ❌ **Aucun `using` croisé** entre les couches internes (`Domaine`/`Application`/
  `Infrastructure`/`Client`) de deux modules différents.
- ❌ Pas d'entité « anémique » : les invariants vivent dans l'entité/agrégat du Domaine, pas
  dans le Handler. L'Application **coordonne**, le Domaine **décide**.
- ❌ Pas d'accès direct, depuis un module, à la base de données d'un autre module.
- ❌ Pas de réintroduction d'un dossier `Module/` intermédiaire ni de SeedWork dupliqué par
  module : la structure de §1 est la référence.

## 3. Communication inter-modules (ADR 0003)
Deux moyens **autorisés et exclusifs**, toujours via le projet `Contracts` :
1. **Contrat public** : une interface exposée dans `<Module>.Contracts`
   (ex : `IVerificateurPermission`), injectée via le `ModuleLoader`.
2. **Bus d'événements interne en mémoire** : publication d'un événement d'intégration défini
   dans `<Module>.Contracts` (ex : `AnnoncePubliee`, `AppelOffrePublie`, implémentant
   `IEvenementIntegration`), consommé par un autre module via un handler. L'émetteur **ne
   connaît pas** ses consommateurs.
Quand on te demande une interaction entre modules, propose toujours l'une de ces deux voies,
jamais un appel direct. Tout type partagé entre modules vit dans `Contracts`, pas ailleurs.

## 4. Couche Domaine — organisation en agrégats (AggregatesModel, doc MS DDD)
Le Domaine est rangé **par agrégat**, pas par type technique :
```
Domaine/
  <Agregat>Agregat/          Un dossier par agrégat (bounded transaction).
    <RacineAgregat>.cs       Racine d'agrégat (hérite de RacineAgregat<TId>). Seule porte
                             d'écriture de l'agrégat ; garante de ses invariants.
    <Entite>.cs              Entités internes de l'agrégat (hérite de Entite).
    <ObjetValeur>.cs         Value objects de l'agrégat (hérite de ObjetValeur).
    IDepot<Agregat>.cs       Interface du dépôt (PORT de persistance) — définie ICI, dans le
                             Domaine. Implémentée dans Infrastructure.
  Enums.cs / Exceptions      Types métier transverses au module.
```
Règles :
- L'**interface de dépôt** appartient au Domaine (un dépôt par racine d'agrégat). Le Domaine
  expose le besoin (`IDepotAppelsOffre`), l'Infrastructure le réalise (`DepotAppelsOffreEfCore`).
- On ne référence un agrégat depuis un autre **que par son identifiant** (`Guid`), jamais par
  navigation directe entre racines.
- Constructeurs privés + fabriques nommées en français (`AppelOffre.Publier(...)`) ; setters
  privés ; aucune donnée mutable exposée publiquement.

## 5. Couche Client — ports d'entrée/sortie + wiring (PAS de définition d'endpoint)
`Client/` est l'**adaptateur HTTP** du module. Il ne contient **aucune route en dur** ni
logique : seulement les ports et le branchement.
```
Client/
  IEndpoint.cs              Port d'ENTRÉE : contrat d'un endpoint (méthode Map). Les endpoints
                            CONCRETS implémentant IEndpoint sont définis dans
                            Application/Features/** et sont seulement DÉCOUVERTS/branchés ici.
  IPresentateur<T>.cs       Port de SORTIE : traduit un résultat de cas d'usage en réponse HTTP.
  <Module>Endpoints.cs      Wiring : crée le groupe de routes (ex. /freelance) et branche
                            chaque IEndpoint résolu via DI. Aucune route déclarée à la main.
```
Règle : la couche Client **n'invente pas** d'endpoint ; elle déclare les ports et câble. La
définition d'un endpoint (mapping DTO ↔ Command/Query) reste dans sa vertical slice.

## 6. Vertical slice (ADR 0006)
Une fonctionnalité = un dossier `Application/Features/<NomCasDUsageFR>/` contenant :
- `<CasDUsage>Command.cs` ou `<CasDUsage>Query.cs` (record),
- `<CasDUsage>Handler.cs` (implémente `IRequestHandler<...>` MediatR),
- `<CasDUsage>Validator.cs` (FluentValidation),
- `<CasDUsage>Endpoint.cs` (implémente `IEndpoint` du Client : mappe DTO ↔ Command/Query).
Nommer les cas d'usage en **français** (ex : `PostulerAnnonce`, `PublierAppelOffre`).

## 7. Ubiquitous Language
Tout type du Domaine et de l'Application porte un nom **français** issu de `CONTEXT.md`
(`AnnonceEmploi`, `CahierDesCharges`, `PropositionFreelance`, `Abonnement`…). La plomberie
(`...Handler`, `...DbContext`, `...Configuration`, `ModuleLoader`) reste en anglais.

## 8. Quand tu ajoutes une fonctionnalité
1. Place-la dans le bon module et la bonne couche (Domaine décide, Application coordonne).
2. Si elle introduit un nouvel agrégat → un dossier `Domaine/<Agregat>Agregat/` avec sa
   racine, ses entités/VO et son `IDepot<Agregat>` ; l'implémentation va dans Infrastructure.
3. Crée la vertical slice complète (Command/Query + Handler + Validator + Endpoint).
4. Si elle touche un autre module → événement d'intégration ou contrat public (via `Contracts`),
   jamais de référence directe au projet interne.
5. Respecte d'abord la skill `cvtech-permissions` (vérif en tête de handler) et `cvtech-tdd`
   (test rouge d'abord).
