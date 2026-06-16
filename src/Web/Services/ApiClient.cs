using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CVTech.Web.Services;

/// <summary>Erreur remontée par l'API (401 non authentifié, 403 permission, 400 validation…).</summary>
public sealed class ApiException(string message) : Exception(message);

/// <summary>
/// Client typé de l'API CVTech. Aucune logique métier : il mappe les appels REST vers des
/// méthodes C#, ajoute le jeton Bearer de la session et traduit les codes HTTP d'erreur.
/// L'identité de l'appelant n'est JAMAIS envoyée dans le corps : elle est portée par le jeton.
/// </summary>
public sealed class ApiClient(HttpClient http, SessionUtilisateur session)
{
    // --- GestionIdentite ---

    public async Task<SessionReponse> InscrireAsync(string email, string motDePasse, RoleUtilisateur role)
    {
        var reponse = await http.PostAsJsonAsync("/identite/inscription", new { email, motDePasse, role });
        await AssurerSuccesAsync(reponse);
        return (await reponse.Content.ReadFromJsonAsync<SessionReponse>())!;
    }

    public async Task<SessionReponse> ConnecterAsync(string email, string motDePasse)
    {
        var reponse = await http.PostAsJsonAsync("/identite/connexion", new { email, motDePasse });
        await AssurerSuccesAsync(reponse);
        return (await reponse.Content.ReadFromJsonAsync<SessionReponse>())!;
    }

    public async Task BloquerCompteAsync(Guid cibleId)
    {
        Authentifier();
        var reponse = await http.PostAsJsonAsync($"/identite/comptes/{cibleId}/blocage", new { });
        await AssurerSuccesAsync(reponse);
    }

    // --- CatalogueEmploi ---

    public async Task<IReadOnlyList<AnnonceVue>> ListerAnnoncesAsync(string? domaine = null)
        => await http.GetFromJsonAsync<IReadOnlyList<AnnonceVue>>(AvecDomaine("/emploi/annonces", domaine)) ?? [];

    public async Task<Guid> PublierAnnonceAsync(
        string titre, string description, TypeContrat typeContrat, string domaineLibelle)
    {
        Authentifier();
        var reponse = await http.PostAsJsonAsync("/emploi/annonces",
            new { titre, description, typeContrat, domaineLibelle });
        await AssurerSuccesAsync(reponse);
        return (await reponse.Content.ReadFromJsonAsync<CreationReponse>())!.Id;
    }

    public async Task<Guid> ConstituerCvAsync(string presentation, IReadOnlyList<string> competences)
    {
        Authentifier();
        var reponse = await http.PostAsJsonAsync("/emploi/cv", new { presentation, competences });
        await AssurerSuccesAsync(reponse);
        return (await reponse.Content.ReadFromJsonAsync<CreationReponse>())!.Id;
    }

    /// <summary>Récupère le CV du candidat connecté, ou null s'il n'en a pas encore constitué (204).</summary>
    public async Task<CvVue?> ObtenirMonCvAsync()
    {
        Authentifier();
        var reponse = await http.GetAsync("/emploi/mon-cv");
        if (reponse.StatusCode == HttpStatusCode.NoContent) return null;
        await AssurerSuccesAsync(reponse);
        return await reponse.Content.ReadFromJsonAsync<CvVue>();
    }

    public async Task<Guid> PostulerAsync(Guid annonceId, string? lettreMotivation)
    {
        Authentifier();
        var reponse = await http.PostAsJsonAsync(
            $"/emploi/annonces/{annonceId}/candidatures", new { lettreMotivation });
        await AssurerSuccesAsync(reponse);
        return (await reponse.Content.ReadFromJsonAsync<CreationReponse>())!.Id;
    }

    // --- AppelOffreFreelance ---

    public async Task<IReadOnlyList<AppelOffreVue>> ListerAppelsOffreAsync(string? domaine = null)
        => await http.GetFromJsonAsync<IReadOnlyList<AppelOffreVue>>(AvecDomaine("/freelance/appels-offre", domaine)) ?? [];

    public async Task<Guid> PublierAppelOffreAsync(
        string titre, string contexte, string livrables,
        DateTimeOffset deadline, decimal budgetMin, decimal budgetMax, string domaineLibelle)
    {
        Authentifier();
        var reponse = await http.PostAsJsonAsync("/freelance/appels-offre",
            new { titre, contexte, livrables, deadline, budgetMin, budgetMax, domaineLibelle });
        await AssurerSuccesAsync(reponse);
        return (await reponse.Content.ReadFromJsonAsync<CreationReponse>())!.Id;
    }

    public async Task<Guid> SoumettrePropositionAsync(
        Guid appelOffreId, decimal montantTJM, int dureeJours, string methodologie)
    {
        Authentifier();
        var reponse = await http.PostAsJsonAsync(
            $"/freelance/appels-offre/{appelOffreId}/propositions",
            new { montantTJM, dureeJours, methodologie });
        await AssurerSuccesAsync(reponse);
        return (await reponse.Content.ReadFromJsonAsync<CreationReponse>())!.Id;
    }

    public async Task SelectionnerLaureatAsync(Guid appelOffreId, Guid propositionId)
    {
        Authentifier();
        var reponse = await http.PostAsJsonAsync(
            $"/freelance/appels-offre/{appelOffreId}/laureat", new { propositionId });
        await AssurerSuccesAsync(reponse);
    }

    // --- ActualiteEtAbonnement ---

    public async Task<Guid> PublierArticleAsync(
        string titre, string contenu, CategorieEditoriale categorie,
        string? domaineLibelle, string? sourceNom, string? sourceUrl)
    {
        Authentifier();
        var reponse = await http.PostAsJsonAsync("/actualite/articles",
            new { titre, contenu, categorie, domaineLibelle, sourceNom, sourceUrl });
        await AssurerSuccesAsync(reponse);
        return (await reponse.Content.ReadFromJsonAsync<CreationReponse>())!.Id;
    }

    public async Task SAbonnerAsync(IReadOnlyList<string> domaines, CanalDiffusion canal)
    {
        Authentifier();
        var reponse = await http.PostAsJsonAsync("/actualite/abonnements", new { domaines, canal });
        await AssurerSuccesAsync(reponse);
    }

    public async Task<IReadOnlyList<NotificationVue>> ListerNotificationsAsync()
    {
        Authentifier();
        return await http.GetFromJsonAsync<IReadOnlyList<NotificationVue>>("/actualite/notifications") ?? [];
    }

    public async Task<string> ObtenirFluxRssAsync(string? domaine = null)
        => await http.GetStringAsync(AvecDomaine("/feed/rss", domaine));

    // --- Outillage interne ---

    /// <summary>Applique le jeton Bearer de la session à l'en-tête Authorization.</summary>
    private void Authentifier() =>
        http.DefaultRequestHeaders.Authorization =
            session.Jeton is { } jeton ? new AuthenticationHeaderValue("Bearer", jeton) : null;

    private static string AvecDomaine(string route, string? domaine)
        => string.IsNullOrWhiteSpace(domaine) ? route : $"{route}?domaine={Uri.EscapeDataString(domaine)}";

    private static async Task AssurerSuccesAsync(HttpResponseMessage reponse)
    {
        if (reponse.IsSuccessStatusCode) return;

        var detail = await LireDetailAsync(reponse);
        var message = reponse.StatusCode switch
        {
            HttpStatusCode.Unauthorized => detail ?? "Authentification requise ou invalide (401).",
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
