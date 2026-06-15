using CVTech.Modules.ActualiteEtAbonnement;
using CVTech.Modules.ActualiteEtAbonnement.Client;
using CVTech.Modules.AppelOffreFreelance;
using CVTech.Modules.AppelOffreFreelance.Client;
using CVTech.Modules.CatalogueEmploi;
using CVTech.Modules.CatalogueEmploi.Client;
using CVTech.Modules.GestionIdentite;
using CVTech.Modules.GestionIdentite.Client;
using CVTech.SharedKernel.Comportements;
using CVTech.SharedKernel.Evenements;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// --- Plomberie transverse ---
// Bus d'événements interne (ADR 0003) + pipeline de validation global (une seule fois).
builder.Services.AddBusEvenements();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// CORS pour le front Blazor WebAssembly.
builder.Services.AddCors(options => options.AddDefaultPolicy(p =>
    p.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed(_ => true).AllowCredentials()));

// --- Enregistrement des 4 modules (composition root global) ---
builder.Services.AddModuleGestionIdentite();
builder.Services.AddModuleCatalogueEmploi();
builder.Services.AddModuleAppelOffreFreelance();
builder.Services.AddModuleActualiteEtAbonnement();

var app = builder.Build();

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
