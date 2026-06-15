# CONTEXT — Glossaire du langage métier (Ubiquitous Language)

> **Règle de discipline (grill-with-docs)** : ce fichier est **uniquement un glossaire**.
> Pas de spécifications, pas de détails d'implémentation, pas de bloc-notes.
> On y inscrit la définition canonique d'un terme métier au moment où il se cristallise.
> Le Domaine et l'Application sont rédigés en **français** ; la plomberie technique
> (Controllers, Handlers, Infrastructure, ModuleLoader) reste en **anglais**.

## Concepts transverses (SharedKernel)

| Terme | Définition |
|---|---|
| **DomaineMetier** | Objet de Valeur partagé identifiant un champ d'expertise (ex : « Cloud Azure », « Data Science », « Cybersécurité », « DevOps »). Exposé via un contrat public ; utilisé par les modules B, C et D. Immuable, comparé par valeur. |
| **IVerificateurPermission** | Contrat public exposé par `GestionIdentite`. Seul point d'autorisation : un handler l'interroge **avant** toute action métier. |
| **RevendicationPermission** | Demande d'autorisation (qui ? quelle action ? sur quelle ressource ?) soumise à `IVerificateurPermission`. |
| **PermissionRefuseeException** | Exception métier levée quand une action est refusée par la matrice de permissions. |
| **Bus d'événements interne** | Mécanisme in-memory permettant à un module de publier un événement consommé par un autre, sans dépendance directe. |

## Module A — GestionIdentite (Profils, Rôles & Permissions)

| Terme | Définition |
|---|---|
| **ProfilCandidat** | Personne cherchant un emploi salarié et/ou des missions freelance. Constitue un CV, postule, soumet des propositions, s'abonne à des domaines. |
| **ProfilEntreprise** | Organisation publiant des annonces d'emploi et des appels d'offre, consultant les candidatures/propositions reçues. |
| **Administrateur** | Utilisateur héritant de tous les droits : modération, publication d'articles, blocage de comptes, gestion du référentiel des domaines. |
| **RoleUtilisateur** | Catégorie de droits : `Candidat`, `Entreprise`, `Administrateur`. |
| **MatricePermissions** | Table associant chaque (rôle, action) à une autorisation ou un refus. Source de vérité des droits. |
| **Compte bloqué** | Compte désactivé par un administrateur ; aucune action métier authentifiée n'est possible. |

## Module B — CatalogueEmploi (Annonces & Candidatures)

| Terme | Définition |
|---|---|
| **AnnonceEmploi** | Offre d'emploi publiée par une entreprise (rattachée à un `DomaineMetier`). Publique et consultable sans compte. |
| **CurriculumVitae** | CV structuré constitué par un candidat, réutilisable pour toutes ses candidatures. |
| **Candidature** | Acte par lequel un candidat postule à une `AnnonceEmploi`, avec lettre de motivation optionnelle. |
| **TypeContrat** | Nature de l'emploi : CDI, CDD, stage, alternance, apprentissage. |
| **AnnoncePubliee** | Événement émis lors de la publication d'une `AnnonceEmploi`, consommé par `ActualiteEtAbonnement`. |

## Module C — AppelOffreFreelance (Missions & Propositions)

| Terme | Définition |
|---|---|
| **AppelOffre** | Mission ponctuelle publiée par une entreprise (rattachée à un `DomaineMetier`). Publique. |
| **CahierDesCharges** | Description d'un `AppelOffre` : contexte, livrables attendus, deadline, fourchette budgétaire. |
| **PropositionFreelance** | Réponse chiffrée d'un candidat indépendant à un `AppelOffre` : TJM, durée, méthodologie. |
| **CritereSelection** | Critère utilisé par l'entreprise pour comparer les propositions et choisir un lauréat. |
| **BaremeTJM** | Référentiel de Taux Journalier Moyen servant à cadrer ou comparer les propositions. |
| **Lauréat** | Candidat dont la `PropositionFreelance` est sélectionnée par l'entreprise. |
| **AppelOffrePublie** | Événement émis lors de la publication d'un `AppelOffre`, consommé par `ActualiteEtAbonnement`. |

## Module D — ActualiteEtAbonnement (Fil RSS éditorial & Notifications)

| Terme | Définition |
|---|---|
| **ArticleActualite** | Contenu éditorial tech publié par un administrateur (ou agrégé). Diffusé via RSS. **N'est jamais** une annonce ni un appel d'offre. |
| **FilActualite** | Ensemble ordonné des `ArticleActualite`, exposé en RSS 2.0 public et anonyme. |
| **SourceExterne** | Origine d'un `ArticleActualite` agrégé depuis l'extérieur (optionnel). |
| **CategorieEditoriale** | Classement éditorial d'un `ArticleActualite`. |
| **Abonnement** | Lien entre un utilisateur authentifié et un ou plusieurs `DomaineMetier` ; déclenche des notifications. |
| **Notification** | Message envoyé à un abonné lorsqu'une annonce/AO paraît dans un domaine suivi. |
| **CanalDiffusion** | Voie d'acheminement d'une `Notification`. **Canal retenu pour ce projet : in-app (SignalR).** |
| **PreferenceNotification** | Choix d'un utilisateur sur le(s) canal(aux) de ses notifications. |
