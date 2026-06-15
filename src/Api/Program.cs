using CVTech.Modules.ActualiteEtAbonnement;
using CVTech.Modules.ActualiteEtAbonnement.Client;
using CVTech.Modules.AppelOffreFreelance;
using CVTech.Modules.AppelOffreFreelance.Client;
using CVTech.Modules.CatalogueEmploi;
using CVTech.Modules.CatalogueEmploi.Client;
using CVTech.Modules.GestionIdentite;
using CVTech.Modules.GestionIdentite.Client;
using CVTech.Api;
using CVTech.SharedKernel.Comportements;
using CVTech.SharedKernel.Evenements;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- Plomberie transverse ---
// Bus d'événements interne (ADR 0003) + pipeline de validation global (une seule fois).
builder.Services.AddBusEvenements();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// CORS pour le front Blazor WebAssembly.
builder.Services.AddCors(options => options.AddDefaultPolicy(p =>
    p.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed(_ => true).AllowCredentials()));

// --- Persistance (ADR 0005) : SQLite en local/dev (un fichier par module), Azure SQL au déploiement.
// Le provider est choisi par configuration ("Persistence:Provider" = "Sqlite" | "SqlServer").
var fournisseur = builder.Configuration["Persistence:Provider"] ?? "Sqlite";
var chaineConnexion = builder.Configuration.GetConnectionString("CVTech");

Action<DbContextOptionsBuilder> Bdd(string schema) => options =>
{
    if (fournisseur.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        options.UseSqlServer(chaineConnexion,
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", schema));
    else
        options.UseSqlite($"Data Source=cvtech-{schema}.db");
};

// --- Enregistrement des 4 modules (composition root global) ---
builder.Services.AddModuleGestionIdentite(Bdd("identite"));
builder.Services.AddModuleCatalogueEmploi(Bdd("emploi"));
builder.Services.AddModuleAppelOffreFreelance(Bdd("freelance"));
builder.Services.AddModuleActualiteEtAbonnement(Bdd("actualite"));

var app = builder.Build();

// Prépare la base au démarrage (ignoré par l'outillage EF design-time : migrations add/script).
if (!EF.IsDesignTime)
    await app.InitialiserBaseDeDonneesAsync(fournisseur);

app.UseCors();

app.MapGet("/", () => "Plateforme-CVTech — API en ligne.");

// --- Endpoints exposés par chaque module (couche Client) ---
app.MapGestionIdentite();
app.MapCatalogueEmploi();
app.MapAppelOffreFreelance();
app.MapActualiteEtAbonnement();

app.Run();

// Rend la classe Program accessible aux tests d'intégration éventuels.
public partial class Program;
