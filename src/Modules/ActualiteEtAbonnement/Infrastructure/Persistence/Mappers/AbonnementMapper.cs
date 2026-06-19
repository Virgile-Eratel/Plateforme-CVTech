using CVTech.Modules.ActualiteEtAbonnement.Domaine;
using CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence.Entities;
using CVTech.SharedKernel.Domaine;

namespace CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence.Mappers;

/// <summary>Conversions entre l'agrégat <see cref="Abonnement"/> et <see cref="AbonnementEntity"/>.</summary>
public static class AbonnementMapper
{
    public static AbonnementEntity ToEntity(Abonnement domaine) => new()
    {
        Id = domaine.Id,
        UtilisateurId = domaine.UtilisateurId,
        Canal = domaine.Canal,
        Domaines = domaine.Domaines
            .Select(d => new AbonnementDomaineEntity { Code = d.Code, Libelle = d.Libelle })
            .ToList()
    };

    public static Abonnement ToDomain(AbonnementEntity entite) =>
        Abonnement.Reconstituer(
            entite.Id,
            entite.UtilisateurId,
            entite.Canal,
            entite.Domaines.Select(d => DomaineMetier.Reconstituer(d.Code, d.Libelle)));

    /// <summary>Reporte l'état mutable de l'agrégat sur une entité déjà suivie (mise à jour).</summary>
    public static void Appliquer(Abonnement domaine, AbonnementEntity entite)
    {
        entite.Canal = domaine.Canal;
        entite.Domaines = domaine.Domaines
            .Select(d => new AbonnementDomaineEntity { Code = d.Code, Libelle = d.Libelle })
            .ToList();
    }
}
