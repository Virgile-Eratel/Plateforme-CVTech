using CVTech.SharedKernel.Domaine;

namespace CVTech.Modules.ActualiteEtAbonnement.Domaine;

/// <summary>
/// Agrégat : abonnement d'un utilisateur à un ou plusieurs domaines métier.
/// Déclenche des notifications à la publication d'une annonce / AO dans ces domaines.
/// </summary>
public sealed class Abonnement : RacineAgregat<Guid>
{
    private readonly List<DomaineMetier> _domaines = [];

    public Guid UtilisateurId { get; private set; }
    public CanalDiffusion Canal { get; private set; }
    public IReadOnlyCollection<DomaineMetier> Domaines => _domaines.AsReadOnly();

    private Abonnement() { }

    public static Abonnement Creer(
        Guid utilisateurId, IEnumerable<DomaineMetier> domaines, CanalDiffusion canal = CanalDiffusion.InApp)
    {
        if (utilisateurId == Guid.Empty)
            throw new ArgumentException("L'utilisateur est obligatoire.", nameof(utilisateurId));

        var abonnement = new Abonnement { Id = Guid.NewGuid(), UtilisateurId = utilisateurId, Canal = canal };
        abonnement.AjouterDomaines(domaines);
        return abonnement;
    }

    /// <summary>
    /// Réhydrate l'agrégat depuis la persistance (mapper Infrastructure) en préservant
    /// l'Id, l'utilisateur, le canal et la liste de domaines suivis.
    /// </summary>
    public static Abonnement Reconstituer(
        Guid id, Guid utilisateurId, CanalDiffusion canal, IEnumerable<DomaineMetier> domaines)
    {
        var abonnement = new Abonnement { Id = id, UtilisateurId = utilisateurId, Canal = canal };
        abonnement._domaines.AddRange(domaines);
        return abonnement;
    }

    public void AjouterDomaines(IEnumerable<DomaineMetier> domaines)
    {
        foreach (var domaine in domaines)
            if (!_domaines.Contains(domaine))
                _domaines.Add(domaine);
    }

    public bool EstAbonneAu(string domaineCode) => _domaines.Any(d => d.Code == domaineCode);
}
