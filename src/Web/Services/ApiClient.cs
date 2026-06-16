using System.Net;
using System.Net.Http.Json;

namespace CVTech.Web.Services;

/// <summary>Erreur métier remontée par l'API (403 permission, 400 validation, etc.).</summary>
public sealed class ApiException(string message) : Exception(message);

/// <summary>
/// Client typé de l'API CVTech. Aucune logique métier ici : il ne fait que mapper les
/// appels REST vers des méthodes C# et traduire les codes HTTP d'erreur en <see cref="ApiException"/>.
/// </summary>
public sealed class ApiClient(HttpClient http)
{
    // --- GestionIdentite ---

    public async Task<Guid> InscrireAsync(string email, RoleUtilisateur role)
    {
        var reponse = await http.PostAsJsonAsync("/identite/inscription", new { email, role });
        await AssurerSuccesAsync(reponse);
        var corps = await reponse.Content.ReadFromJsonAsync<CreationReponse>();
        return corps!.Id;
    }

    public async Task BloquerCompteAsync(Guid appelantId, Guid cibleId)
    {
        var reponse = await http.PostAsJsonAsync(
            $"/identite/comptes/{cibleId}/blocage", new { appelantId });
        await AssurerSuccesAsync(reponse);
    }

    // --- CatalogueEmploi ---

    public async Task<IReadOnlyList<AnnonceVue>> ListerAnnoncesAsync(string? domaine = null)
        => await http.GetFromJsonAsync<IReadOnlyList<AnnonceVue>>(AvecDomaine("/emploi/annonces", domaine))
           ?? [];

    public async Task<Guid> PublierAnnonceAsync(
        Guid entrepriseId, string titre, string description, TypeContrat typeContrat, string domaineLibelle)
    {
        var reponse = await http.PostAsJsonAsync("/emploi/annonces",
            new { entrepriseId, titre, description, typeContrat, domaineLibelle });
        await AssurerSuccesAsync(reponse);
        return (await reponse.Content.ReadFromJsonAsync<CreationReponse>())!.Id;
    }

    public async Task<Guid> ConstituerCvAsync(Guid candidatId, string presentation, IReadOnlyList<string> competences)
    {
        var reponse = await http.PostAsJsonAsync("/emploi/cv",
            new { candidatId, presentation, competences });
        await AssurerSuccesAsync(reponse);
        return (await reponse.Content.ReadFromJsonAsync<CreationReponse>())!.Id;
    }

    public async Task<Guid> PostulerAsync(Guid candidatId, Guid annonceId, string? lettreMotivation)
    {
        var reponse = await http.PostAsJsonAsync(
            $"/emploi/annonces/{annonceId}/candidatures", new { candidatId, lettreMotivation });
        await AssurerSuccesAsync(reponse);
        return (await reponse.Content.ReadFromJsonAsync<CreationReponse>())!.Id;
    }

    // --- AppelOffreFreelance ---

    public async Task<IReadOnlyList<AppelOffreVue>> ListerAppelsOffreAsync(string? domaine = null)
        => await http.GetFromJsonAsync<IReadOnlyList<AppelOffreVue>>(AvecDomaine("/freelance/appels-offre", domaine))
           ?? [];

    public async Task<Guid> PublierAppelOffreAsync(
        Guid entrepriseId, string titre, string contexte, string livrables,
        DateTimeOffset deadline, decimal budgetMin, decimal budgetMax, string domaineLibelle)
    {
        var reponse = await http.PostAsJsonAsync("/freelance/appels-offre",
            new { entrepriseId, titre, contexte, livrables, deadline, budgetMin, budgetMax, domaineLibelle });
        await AssurerSuccesAsync(reponse);
        return (await reponse.Content.ReadFromJsonAsync<CreationReponse>())!.Id;
    }

    public async Task<Guid> SoumettrePropositionAsync(
        Guid candidatId, Guid appelOffreId, decimal montantTJM, int dureeJours, string methodologie)
    {
        var reponse = await http.PostAsJsonAsync(
            $"/freelance/appels-offre/{appelOffreId}/propositions",
            new { candidatId, montantTJM, dureeJours, methodologie });
        await AssurerSuccesAsync(reponse);
        return (await reponse.Content.ReadFromJsonAsync<CreationReponse>())!.Id;
    }

    public async Task SelectionnerLaureatAsync(Guid appelantId, Guid appelOffreId, Guid propositionId)
    {
        var reponse = await http.PostAsJsonAsync(
            $"/freelance/appels-offre/{appelOffreId}/laureat", new { appelantId, propositionId });
        await AssurerSuccesAsync(reponse);
    }

    // --- ActualiteEtAbonnement ---

    public async Task<Guid> PublierArticleAsync(
        Guid auteurId, string titre, string contenu, CategorieEditoriale categorie,
        string? domaineLibelle, string? sourceNom, string? sourceUrl)
    {
        var reponse = await http.PostAsJsonAsync("/actualite/articles",
            new { auteurId, titre, contenu, categorie, domaineLibelle, sourceNom, sourceUrl });
        await AssurerSuccesAsync(reponse);
        return (await reponse.Content.ReadFromJsonAsync<CreationReponse>())!.Id;
    }

    public async Task SAbonnerAsync(Guid utilisateurId, IReadOnlyList<string> domaines, CanalDiffusion canal)
    {
        var reponse = await http.PostAsJsonAsync("/actualite/abonnements",
            new { utilisateurId, domaines, canal });
        await AssurerSuccesAsync(reponse);
    }

    public async Task<IReadOnlyList<NotificationVue>> ListerNotificationsAsync(Guid utilisateurId)
        => await http.GetFromJsonAsync<IReadOnlyList<NotificationVue>>($"/actualite/notifications/{utilisateurId}")
           ?? [];

    public async Task<string> ObtenirFluxRssAsync(string? domaine = null)
        => await http.GetStringAsync(AvecDomaine("/feed/rss", domaine));

    // --- Outillage interne ---

    private static string AvecDomaine(string route, string? domaine)
        => string.IsNullOrWhiteSpace(domaine) ? route : $"{route}?domaine={Uri.EscapeDataString(domaine)}";

    private static async Task AssurerSuccesAsync(HttpResponseMessage reponse)
    {
        if (reponse.IsSuccessStatusCode) return;

        var detail = await LireDetailAsync(reponse);
        var message = reponse.StatusCode switch
        {
            HttpStatusCode.Forbidden => detail ?? "Action interdite pour votre rôle (403).",
            HttpStatusCode.BadRequest => detail ?? "Requête invalide (400).",
            _ => detail ?? $"Erreur API ({(int)reponse.StatusCode})."
        };
        throw new ApiException(message);
    }

    private static async Task<string?> LireDetailAsync(HttpResponseMessage reponse)
    {
        try
        {
            // Les erreurs métier sont renvoyées via Results.Problem → ProblemDetails.
            var probleme = await reponse.Content.ReadFromJsonAsync<ProblemeDetail>();
            return string.IsNullOrWhiteSpace(probleme?.Detail) ? probleme?.Title : probleme.Detail;
        }
        catch
        {
            return null;
        }
    }

    private sealed record ProblemeDetail(string? Title, string? Detail);
}
