namespace CVTech.Web.Services;

// --- Énumérations miroir de l'API (sérialisées en entiers par défaut, JsonDefaults.Web) ---

public enum RoleUtilisateur { Candidat, Entreprise, Administrateur }

public enum TypeContrat { CDI, CDD, Stage, Alternance, Apprentissage }

public enum CategorieEditoriale { TendancesRecrutement, EvolutionsSalariales, Frameworks, RetourExperience }

public enum CanalDiffusion { InApp, Email }

// --- Vues de lecture renvoyées par l'API ---

public sealed record AnnonceVue(
    Guid Id, string Titre, string Description, string TypeContrat, string Domaine, DateTimeOffset DatePublication);

public sealed record AppelOffreVue(
    Guid Id, string Titre, string Contexte, string Livrables, DateTimeOffset Deadline,
    decimal BudgetMin, decimal BudgetMax, string Domaine, string Statut, DateTimeOffset DatePublication);

public sealed record NotificationVue(
    Guid Id, string Titre, string Message, bool Lu, DateTimeOffset DateCreation);

public sealed record CvVue(Guid Id, string Presentation, IReadOnlyList<string> Competences, int? Age);

// --- Notification poussée en temps réel par le hub SignalR (méthode "RecevoirNotification") ---

public sealed record NotificationTempsReel(Guid Id, string Titre, string Message, DateTimeOffset DateCreation);

// --- Réponse standard { id } des créations ---

public sealed record CreationReponse(Guid Id);

// --- Réponse d'inscription / connexion (jeton JWT) ---

public sealed record SessionReponse(Guid Id, string Email, string Role, string Jeton);
