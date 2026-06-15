---
name: cvtech-tdd
description: Protocole TDD du projet Plateforme-CVTech. À consulter pour TOUTE écriture de code de production ou de test sous tests/** et src/Modules/** : écrire d'abord le test rouge (Red → Green → Refactor), nommer les tests en français décrivant une règle métier, utiliser xUnit + FluentAssertions + NSubstitute, et couvrir en priorité invariants du Domaine, permissions, événements, RSS et notifications.
---

# PROTOCOLE TDD

Le code de production ne s'écrit **jamais** avant un test rouge qui le justifie. Cycle
imposé : **Red → Green → Refactor**.

S'applique à : `tests/**/*.cs` (et au code de production qu'ils justifient).

## 1. Ordre de génération (non négociable)
1. **RED** — écrire d'abord le(s) test(s) xUnit décrivant la règle métier. Le test doit
   **échouer ou ne pas compiler** car le code de production n'existe pas encore. C'est normal
   et attendu.
2. **GREEN** — écrire le **minimum** de code de production pour faire passer le test.
3. **REFACTOR** — nettoyer sans changer le comportement ; les tests restent verts.

Quand on te demande une fonctionnalité, **commence toujours par proposer le test**, puis
attends/écris le code. Ne génère jamais le code de production d'un cas d'usage sans son test.

## 2. Outils
- **xUnit** comme framework de test.
- **FluentAssertions** pour les assertions (`resultat.Should().Be(...)`,
  `action.Should().Throw<PermissionRefuseeException>()`).
- **NSubstitute** (ou Moq) pour simuler les contrats (`IVerificateurPermission`, dépôts).

## 3. Nommage des tests — en français, décrivant une règle métier
Le nom de la méthode décrit **un comportement métier**, pas une méthode technique.
Format recommandé : `Sujet_Condition_RésultatAttendu` ou phrase métier directe.

Exemples valides :
- `UnCandidatBloquéNePeutPasPostuler`
- `UneEntrepriseNePeutPasPostulerAUneAnnonce`
- `PublierUneAnnonceÉmetLévénementAnnoncePubliee`
- `UnAbonnéAuDomaineCloudAzureEstNotifiéÀLaPublication`
- `LeFluxRssNeContientAucuneAnnonceNiAppelDOffre`
- `UneCandidatureSansLettreDeMotivationEstAcceptée`

Interdits : `Test1`, `TestPostuler`, `HandleShouldWork`.

## 4. Structure d'un test (Arrange / Act / Assert)
```csharp
[Fact]
public async Task UnCandidatBloquéNePeutPasPostuler()
{
    // Arrange
    var permissions = Substitute.For<IVerificateurPermission>();
    permissions.ExigerAsync(Arg.Any<Guid>(), ActionMetier.PostulerAnnonce, Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
               .Returns<Task>(_ => throw new PermissionRefuseeException());
    var handler = new PostulerAnnonceHandler(permissions, /* dépôts simulés */);

    // Act
    Func<Task> action = () => handler.Handle(new PostulerAnnonceCommand(/*...*/), CancellationToken.None);

    // Assert
    await action.Should().ThrowAsync<PermissionRefuseeException>();
}
```
Utiliser `[Theory]` + `[InlineData]` quand la même règle se décline sur plusieurs rôles/cas.

## 5. Ce que les tests doivent couvrir en priorité
- **Invariants du Domaine** : règles d'agrégat, value objects (ex : un `BaremeTJM` négatif est
  refusé, un `DomaineMetier` vide est invalide).
- **Permissions** : pour chaque action protégée, un test prouve le **refus** d'un rôle non
  autorisé et d'un **compte bloqué** (voir la skill `cvtech-permissions`).
- **Événements** : publier une annonce/AO émet bien l'événement attendu sur le bus.
- **RSS** : le flux ne contient que des `ArticleActualite`, jamais d'annonce ni d'AO.
- **Notifications** : seuls les abonnés du bon `DomaineMetier` sont notifiés.

## 6. Organisation
Un projet de test par module : `tests/<Module>.Tests/`. Les tests du Domaine n'ont aucune
dépendance à l'infrastructure (pas de base réelle) ; les dépendances externes sont simulées.
