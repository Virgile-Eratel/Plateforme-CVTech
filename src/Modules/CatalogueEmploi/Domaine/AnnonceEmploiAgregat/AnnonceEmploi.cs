using CVTech.SharedKernel.Domaine;

namespace CVTech.Modules.CatalogueEmploi.Domaine;

/// <summary>Agrégat : offre d'emploi publiée par une entreprise (publique).</summary>
public sealed class AnnonceEmploi : RacineAgregat<Guid>
{
    public Guid EntrepriseId { get; private set; }
    public string Titre { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public TypeContrat TypeContrat { get; private set; }
    public DomaineMetier Domaine { get; private set; } = default!;
    public DateTimeOffset DatePublication { get; private set; }

    private AnnonceEmploi() { }

    public static AnnonceEmploi Publier(
        Guid entrepriseId, string titre, string description, TypeContrat typeContrat, DomaineMetier domaine)
    {
        if (entrepriseId == Guid.Empty)
            throw new ArgumentException("L'entreprise est obligatoire.", nameof(entrepriseId));
        if (string.IsNullOrWhiteSpace(titre))
            throw new ArgumentException("Le titre de l'annonce est obligatoire.", nameof(titre));

        return new AnnonceEmploi
        {
            Id = Guid.NewGuid(),
            EntrepriseId = entrepriseId,
            Titre = titre.Trim(),
            Description = description?.Trim() ?? string.Empty,
            TypeContrat = typeContrat,
            Domaine = domaine,
            DatePublication = DateTimeOffset.UtcNow
        };
    }
}
