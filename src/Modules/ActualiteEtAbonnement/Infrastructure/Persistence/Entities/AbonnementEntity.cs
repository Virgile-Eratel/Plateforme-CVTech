using CVTech.Modules.ActualiteEtAbonnement.Domaine;

namespace CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence.Entities;

/// <summary>
/// Entité de persistance EF Core (DAO plat) de l'agrégat <see cref="Abonnement"/>.
/// La collection de domaines suivis est conservée via une table possédée
/// (<see cref="AbonnementDomaineEntity"/>) afin de préserver le schéma existant.
/// </summary>
public sealed class AbonnementEntity
{
    public Guid Id { get; set; }
    public Guid UtilisateurId { get; set; }
    public CanalDiffusion Canal { get; set; }
    public List<AbonnementDomaineEntity> Domaines { get; set; } = [];
}

/// <summary>Ligne possédée : un domaine métier (code + libellé) suivi par un abonnement.</summary>
public sealed class AbonnementDomaineEntity
{
    public string Code { get; set; } = default!;
    public string Libelle { get; set; } = default!;
}
