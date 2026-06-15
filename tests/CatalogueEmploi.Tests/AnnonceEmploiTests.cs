using CVTech.Modules.CatalogueEmploi.Domaine;
using CVTech.SharedKernel.Domaine;
using FluentAssertions;

namespace CVTech.Modules.CatalogueEmploi.Tests;

public class AnnonceEmploiTests
{
    [Fact]
    public void UneAnnonceSansTitreEstRefusée()
    {
        Action publication = () => AnnonceEmploi.Publier(
            Guid.NewGuid(), "  ", "desc", TypeContrat.CDI, DomaineMetier.Creer("Cloud Azure"));

        publication.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void LeDomaineMetierEstSlugifiéÀLaCréation()
    {
        var domaine = DomaineMetier.Creer("Cloud Azure");

        domaine.Code.Should().Be("cloud-azure");
        domaine.Libelle.Should().Be("Cloud Azure");
    }
}
