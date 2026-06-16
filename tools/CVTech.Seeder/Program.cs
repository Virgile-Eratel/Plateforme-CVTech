using Bogus;
using CVTech.Modules.ActualiteEtAbonnement.Domaine;
using CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence;
using CVTech.Modules.AppelOffreFreelance.Domaine;
using CVTech.Modules.AppelOffreFreelance.Infrastructure.Persistence;
using CVTech.Modules.CatalogueEmploi.Domaine;
using CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence;
using CVTech.Modules.GestionIdentite.Domaine;
using CVTech.Modules.GestionIdentite.Infrastructure;
using CVTech.Modules.GestionIdentite.Infrastructure.Persistence;
using CVTech.Seeder;
using CVTech.SharedKernel.Domaine;
using Microsoft.EntityFrameworkCore;

const string MotDePasseDemo = "Demo!2026";

// --- Arguments : chaîne de connexion (positionnel ou variable d'env) + options ---
var force = args.Contains("--force");
var dryRun = args.Contains("--dry-run"); // génère tout sans écrire en base (validation)
var chaine = args.FirstOrDefault(a => !a.StartsWith("--"))
             ?? Environment.GetEnvironmentVariable("ConnectionStrings__CVTech");

if (string.IsNullOrWhiteSpace(chaine) && !dryRun)
{
    Console.Error.WriteLine(
        """
        Usage : dotnet run --project tools/CVTech.Seeder -- "<chaîne de connexion Azure SQL>" [--force]
        Ou via variable d'environnement : ConnectionStrings__CVTech="..." dotnet run --project tools/CVTech.Seeder
        Option --force : vide les tables existantes avant de réinsérer.
        """);
    return 1;
}

DbContextOptions<T> Options<T>() where T : DbContext =>
    new DbContextOptionsBuilder<T>().UseSqlServer(chaine).Options;

// --- Idempotence : on ne réinsère pas par-dessus des données existantes ---
if (!dryRun)
{
    await using var verif = new IdentiteDbContext(Options<IdentiteDbContext>());
    bool dejaPeuple;
    try { dejaPeuple = await verif.Utilisateurs.AnyAsync(); }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Impossible d'interroger la base (schéma absent ?). " +
                                $"Déploie l'app une fois pour appliquer les migrations.\nDétail : {ex.Message}");
        return 2;
    }

    if (dejaPeuple && !force)
    {
        Console.WriteLine("Base déjà peuplée. Relance avec --force pour vider et régénérer.");
        return 0;
    }
    if (dejaPeuple && force)
    {
        Console.WriteLine("--force : purge des données existantes…");
        await ViderAsync(chaine!);
    }
}
else
{
    Console.WriteLine("--dry-run : génération sans écriture en base.");
}

var faker = new Faker("fr");
var hacheur = new HacheurMotDePasseIdentity();
var hashDemo = hacheur.Hacher(MotDePasseDemo);

// Domaines métier réutilisés dans tous les modules (value object owned, recréé à chaque usage).
DomaineMetier UnDomaine() => DomaineMetier.Creer(faker.PickRandom(Catalogues.Domaines));

// =========================================================================================
// 1) IDENTITÉ — 1 admin + 15 entreprises + 40 candidats (comptes de démo déterministes)
// =========================================================================================
// Provisionnement administratif : Creer autorise tout rôle (dont Administrateur),
// contrairement à l'inscription publique qui refuse Administrateur.
Utilisateur Compte(string email, RoleUtilisateur role)
{
    var u = Utilisateur.Creer(email, role);
    u.DefinirMotDePasse(hashDemo);
    return u;
}

var admin = Compte("admin@cvtech.fr", RoleUtilisateur.Administrateur);

var entreprises = new List<(Guid Id, string Nom)>();
var comptesEntreprise = new List<Utilisateur> { };
{
    var premier = Compte("entreprise@cvtech.fr", RoleUtilisateur.Entreprise);
    comptesEntreprise.Add(premier);
    entreprises.Add((premier.Id, "CVTech Solutions"));
    for (var i = 1; i < 15; i++)
    {
        var nom = faker.Company.CompanyName();
        var u = Compte($"contact@{Slug(nom)}{i}.fr", RoleUtilisateur.Entreprise);
        comptesEntreprise.Add(u);
        entreprises.Add((u.Id, nom));
    }
}

var candidats = new List<(Guid Id, string Nom)>();
var comptesCandidat = new List<Utilisateur>();
{
    var premier = Compte("candidat@cvtech.fr", RoleUtilisateur.Candidat);
    comptesCandidat.Add(premier);
    candidats.Add((premier.Id, faker.Name.FullName()));
    for (var i = 1; i < 40; i++)
    {
        var nom = faker.Name.FullName();
        var u = Compte($"{Slug(nom)}{i}@mail.fr", RoleUtilisateur.Candidat);
        comptesCandidat.Add(u);
        candidats.Add((u.Id, nom));
    }
}

if (!dryRun)
{
    await using var ctx = new IdentiteDbContext(Options<IdentiteDbContext>());
    ctx.Utilisateurs.Add(admin);
    ctx.Utilisateurs.AddRange(comptesEntreprise);
    ctx.Utilisateurs.AddRange(comptesCandidat);
    await ctx.SaveChangesAsync();
}
Console.WriteLine($"Identité    : {1 + comptesEntreprise.Count + comptesCandidat.Count} comptes");

// =========================================================================================
// 2) EMPLOI — 30 annonces, 30 CV, ~60 candidatures
// =========================================================================================
var annonces = new List<AnnonceEmploi>();
for (var i = 0; i < 30; i++)
{
    var ent = faker.PickRandom(entreprises);
    var poste = faker.PickRandom(Catalogues.Postes);
    var seniorite = faker.PickRandom(Catalogues.Seniorites);
    var domaine = UnDomaine();
    var stack = string.Join(", ", faker.PickRandom(Catalogues.Competences, 4));
    var description =
        $"{ent.Nom} recherche un(e) {poste} {seniorite} pour renforcer son équipe {domaine.Libelle}. " +
        $"Vous interviendrez sur : {stack}. Méthodologie {faker.PickRandom(Catalogues.Methodologies)}. " +
        "Télétravail partiel et tickets restaurant.";
    annonces.Add(AnnonceEmploi.Publier(
        ent.Id, $"{poste} {seniorite}", description,
        faker.PickRandom<TypeContrat>(), domaine));
}

var cvs = new List<CurriculumVitae>();
foreach (var cand in faker.PickRandom(candidats, 30))
{
    var poste = faker.PickRandom(Catalogues.Postes);
    var seniorite = faker.PickRandom(Catalogues.Seniorites);
    var presentation =
        $"{poste} {seniorite} ({faker.Random.Int(1, 12)} ans d'expérience), passionné(e) par " +
        $"{faker.PickRandom(Catalogues.Domaines)}. Disponible sous {faker.Random.Int(1, 3)} mois.";
    cvs.Add(CurriculumVitae.Constituer(
        cand.Id, presentation, faker.PickRandom(Catalogues.Competences, faker.Random.Int(4, 8))));
}

var candidatures = new List<Candidature>();
var pairesCandidature = new HashSet<(Guid, Guid)>();
for (var i = 0; i < 60; i++)
{
    var annonce = faker.PickRandom(annonces);
    var cand = faker.PickRandom(candidats);
    if (!pairesCandidature.Add((annonce.Id, cand.Id))) continue; // pas deux fois la même paire
    var lettre = faker.Random.Bool(0.7f)
        ? $"Bonjour, votre poste de {annonce.Titre} correspond à mon profil. {faker.Lorem.Sentence(12)}"
        : null;
    candidatures.Add(Candidature.Deposer(annonce.Id, cand.Id, lettre));
}

if (!dryRun)
{
    await using var ctx = new EmploiDbContext(Options<EmploiDbContext>());
    ctx.Annonces.AddRange(annonces);
    ctx.Cvs.AddRange(cvs);
    ctx.Candidatures.AddRange(candidatures);
    await ctx.SaveChangesAsync();
}
Console.WriteLine($"Emploi      : {annonces.Count} annonces, {cvs.Count} CV, {candidatures.Count} candidatures");

// =========================================================================================
// 3) FREELANCE — 15 appels d'offres, ~30 propositions, quelques attributions
// =========================================================================================
var appels = new List<AppelOffre>();
for (var i = 0; i < 15; i++)
{
    var ent = faker.PickRandom(entreprises);
    var poste = faker.PickRandom(Catalogues.Postes);
    var domaine = UnDomaine();
    var budgetMin = faker.Random.Int(15, 35) * 1000m;
    var cdc = CahierDesCharges.Creer(
        contexte: $"{ent.Nom} lance une mission autour de {domaine.Libelle}. {faker.Lorem.Sentence(15)}",
        livrables: $"Livrables attendus : {faker.Lorem.Sentence(10)}",
        deadline: faker.Date.FutureOffset(1, DateTimeOffset.UtcNow.AddDays(30)),
        budgetMin: budgetMin,
        budgetMax: budgetMin + faker.Random.Int(10, 40) * 1000m);
    appels.Add(AppelOffre.Publier(ent.Id, $"Mission freelance : {poste}", cdc, domaine));
}

var propositions = new List<PropositionFreelance>();
var pairesProposition = new HashSet<(Guid, Guid)>();
for (var i = 0; i < 30; i++)
{
    var appel = faker.PickRandom(appels);
    var cand = faker.PickRandom(candidats);
    if (!pairesProposition.Add((appel.Id, cand.Id))) continue;
    propositions.Add(PropositionFreelance.Soumettre(
        appel.Id, cand.Id,
        BaremeTJM.Creer(faker.Random.Int(350, 800)),
        faker.Random.Int(20, 120),
        $"Approche {faker.PickRandom(Catalogues.Methodologies)}. {faker.Lorem.Sentence(12)}"));
}

// Attribue ~6 appels qui ont reçu au moins une proposition.
foreach (var appel in faker.PickRandom(appels, 6))
{
    var laureate = propositions.FirstOrDefault(p => p.AppelOffreId == appel.Id);
    if (laureate is not null) appel.SelectionnerLaureat(laureate.Id);
}

if (!dryRun)
{
    await using var ctx = new FreelanceDbContext(Options<FreelanceDbContext>());
    ctx.AppelsOffre.AddRange(appels);
    ctx.Propositions.AddRange(propositions);
    await ctx.SaveChangesAsync();
}
Console.WriteLine($"Freelance   : {appels.Count} appels d'offres, {propositions.Count} propositions");

// =========================================================================================
// 4) ACTUALITÉ — articles éditoriaux, abonnements, notifications
// =========================================================================================
var articles = Catalogues.Articles.Select(a =>
{
    var source = faker.Random.Bool(0.6f)
        ? Catalogues.Sources[faker.Random.Int(0, Catalogues.Sources.Length - 1)] is var s
            ? SourceExterne.Creer(s.Nom, s.Url) : null
        : null;
    var contenu = string.Join("\n\n", faker.Lorem.Paragraphs(3));
    return ArticleActualite.Publier(
        admin.Id, a.Titre, contenu, (CategorieEditoriale)a.Categorie,
        faker.Random.Bool(0.7f) ? UnDomaine() : null, source);
}).ToList();

var abonnements = faker.PickRandom(candidats, 30).Select(c =>
    Abonnement.Creer(
        c.Id,
        Enumerable.Range(0, faker.Random.Int(1, 3)).Select(_ => UnDomaine()).DistinctBy(d => d.Code),
        faker.PickRandom<CanalDiffusion>())).ToList();

var notifications = new List<Notification>();
for (var i = 0; i < 20; i++)
{
    var cand = faker.PickRandom(candidats);
    var n = Notification.Creer(
        cand.Id,
        "Nouvelle activité dans vos domaines",
        $"Un nouvel article « {faker.PickRandom(Catalogues.Articles).Titre} » pourrait vous intéresser.",
        faker.PickRandom<CanalDiffusion>());
    if (faker.Random.Bool(0.4f)) n.MarquerLu();
    notifications.Add(n);
}

if (!dryRun)
{
    await using var ctx = new ActualiteDbContext(Options<ActualiteDbContext>());
    ctx.Articles.AddRange(articles);
    ctx.Abonnements.AddRange(abonnements);
    ctx.Notifications.AddRange(notifications);
    await ctx.SaveChangesAsync();
}
Console.WriteLine($"Actualité   : {articles.Count} articles, {abonnements.Count} abonnements, {notifications.Count} notifications");

Console.WriteLine($"""

    ✅ Données de démo générées.
    Comptes de test (mot de passe : {MotDePasseDemo}) :
      • admin@cvtech.fr       (Administrateur)
      • entreprise@cvtech.fr  (Entreprise)
      • candidat@cvtech.fr    (Candidat)
    """);
return 0;

// --- Helpers ---

// Vide les tables enfant avant parent pour respecter les clés étrangères de chaque schéma.
static async Task ViderAsync(string chaine)
{
    DbContextOptions<T> O<T>() where T : DbContext =>
        new DbContextOptionsBuilder<T>().UseSqlServer(chaine).Options;

    await using var emploi = new EmploiDbContext(O<EmploiDbContext>());
    await emploi.Candidatures.ExecuteDeleteAsync();
    await emploi.Cvs.ExecuteDeleteAsync();
    await emploi.Annonces.ExecuteDeleteAsync();

    await using var freelance = new FreelanceDbContext(O<FreelanceDbContext>());
    await freelance.Propositions.ExecuteDeleteAsync();
    await freelance.AppelsOffre.ExecuteDeleteAsync();

    await using var actualite = new ActualiteDbContext(O<ActualiteDbContext>());
    await actualite.Notifications.ExecuteDeleteAsync();
    await actualite.Abonnements.ExecuteDeleteAsync();
    await actualite.Articles.ExecuteDeleteAsync();

    await using var identite = new IdentiteDbContext(O<IdentiteDbContext>());
    await identite.Utilisateurs.ExecuteDeleteAsync();
}

// Slug ASCII minuscule pour fabriquer des emails valides et uniques.
static string Slug(string valeur)
{
    var sansAccents = new string(valeur.Normalize(System.Text.NormalizationForm.FormD)
        .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                    != System.Globalization.UnicodeCategory.NonSpacingMark)
        .ToArray());
    var slug = new string(sansAccents.ToLowerInvariant()
        .Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray())
        .Trim('-');
    while (slug.Contains("--")) slug = slug.Replace("--", "-");
    return string.IsNullOrEmpty(slug) ? "x" : slug;
}
