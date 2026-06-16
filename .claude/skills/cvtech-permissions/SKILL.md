---
name: cvtech-permissions
description: Règles de contrôle des permissions du projet Plateforme-CVTech. À consulter pour TOUT cas d'usage sous src/Modules/**/Application/Features/** : chaque Handler d'action protégée doit appeler IVerificateurPermission.ExigerAsync en première ligne, traduire un refus en HTTP 403, vérifier la propriété de la ressource, refuser tout compte bloqué, et respecter la matrice de permissions (Candidat / Entreprise / Administrateur) qui est la source de vérité.
---

# CONTEXTE
Trois rôles cohabitent : **Candidat**, **Entreprise**, **Administrateur**. Aucune action
métier ne doit s'exécuter sans avoir interrogé en amont le contrat `IVerificateurPermission`
exposé par le module `GestionIdentite` (ADR 0004). Les annonces et appels d'offre sont
consultables **anonymement** ; toute action (CV, candidature, proposition, abonnement,
publication, modération) exige une autorisation.

S'applique à : `src/Modules/**/Application/Features/**/*.cs`.

# INSTRUCTIONS

## 1. Règle absolue — vérification en première ligne
Dans **tout** `Handler` de `Application/Features/**` qui exécute une action protégée, la
**première instruction exécutable** du `Handle(...)` est l'appel à `IVerificateurPermission`.
Aucune lecture de base, aucun calcul métier, aucune mutation ne doit la précéder.
Si le handler généré ne respecte pas cela, **considère-le comme invalide et corrige-le**.

```csharp
public async Task<Guid> Handle(PostulerAnnonceCommand commande, CancellationToken ct)
{
    // 1. PERMISSION D'ABORD — toujours en tête, sans exception
    await _permissions.ExigerAsync(
        commande.UtilisateurId, ActionMetier.PostulerAnnonce, ressource: commande.AnnonceId, ct);

    // 2. ... seulement ensuite l'action métier
}
```

## 2. Le contrat
`GestionIdentite` expose dans `GestionIdentite.Contracts` :
```csharp
public interface IVerificateurPermission
{
    Task<bool> EstAutoriseAsync(Guid utilisateurId, ActionMetier action, Guid? ressource = null, CancellationToken ct = default);
    // Lève PermissionRefuseeException si non autorisé :
    Task ExigerAsync(Guid utilisateurId, ActionMetier action, Guid? ressource = null, CancellationToken ct = default);
}
```
`ActionMetier` est une énumération tirée **directement** de la matrice de permissions du
README. Les autres modules dépendent de `GestionIdentite.Contracts` uniquement — jamais de
l'implémentation ni de la base de `GestionIdentite`.

## 3. Échec d'autorisation
En cas de refus, lever une **exception métier** `PermissionRefuseeException` (définie dans le
SharedKernel ou le Domaine de GestionIdentite), traduite par la couche Client en
**HTTP 403 Forbidden** (et 401 si non authentifié). Ne jamais retourner un résultat vide
silencieux.

## 4. Traduction de la matrice du README → code
La matrice de permissions du README est la **source de vérité**. Elle se traduit dans
`MatricePermissions` (module GestionIdentite). Référence à respecter :

| ActionMetier | Candidat | Entreprise | Administrateur |
|---|:--:|:--:|:--:|
| `ConstituerCv` | ✅ | ❌ | ✅ |
| `PostulerAnnonce` | ✅ | ❌ | ❌ |
| `SoumettreProposition` | ✅ | ❌ | ❌ |
| `PublierAnnonce` | ❌ | ✅ (les siennes) | ✅ |
| `PublierAppelOffre` | ❌ | ✅ (les siens) | ✅ |
| `ConsulterCandidaturesRecues` | ❌ | ✅ (siennes) | ✅ |
| `SAbonnerDomaine` | ✅ | ✅ | ✅ |
| `PublierArticleActualite` | ❌ | ❌ | ✅ |
| `ModererAnnonceOuAppelOffre` | ❌ | ❌ | ✅ |
| `BloquerOuReactiverCompte` | ❌ | ❌ | ✅ |
| `GererReferentielDomaines` | ❌ | ❌ | ✅ |

Les actions « consulter une annonce/AO » et « consulter le RSS » sont **publiques** : pas
d'appel de permission, l'endpoint est anonyme.

## 5. Propriété de la ressource
Pour les actions marquées « (les siennes) », l'autorisation ne suffit pas au rôle : vérifier
aussi que la ressource **appartient** à l'appelant (une entreprise ne publie/consulte que
ses propres annonces). Le `ExigerAsync(..., ressource: ...)` doit porter cette vérification.

## 6. Compte bloqué
Un compte bloqué (cf. `CONTEXT.md`) échoue à **toute** action authentifiée :
`IVerificateurPermission` refuse systématiquement, quel que soit le rôle.

## 7. Preuve par test
Chaque action protégée doit être accompagnée d'un test prouvant qu'elle est **refusée** pour
un rôle non autorisé ou un compte bloqué (ex : `UnCandidatBloqueNePeutPasPostuler`,
`UneEntrepriseNePeutPasPostuler`). Voir la skill `cvtech-tdd`.
