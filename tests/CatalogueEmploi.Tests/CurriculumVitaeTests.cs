using CVTech.Modules.CatalogueEmploi.Domaine;
using FluentAssertions;

namespace CVTech.Modules.CatalogueEmploi.Tests;

public class CurriculumVitaeTests
{
    [Fact]
    public void UnCvSansPrésentationEstRefusé()
    {
        Action action = () => CurriculumVitae.Constituer(Guid.NewGuid(), "   ", new[] { "C#" });

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MettreÀJourRemplaceLaPrésentationEtLesCompétences()
    {
        var cv = CurriculumVitae.Constituer(Guid.NewGuid(), "Présentation initiale", new[] { "C#" });

        cv.MettreAJour("Nouvelle présentation", new[] { "Azure", "Terraform" });

        cv.Presentation.Should().Be("Nouvelle présentation");
        cv.Competences.Should().BeEquivalentTo("Azure", "Terraform");
    }

    [Fact]
    public void MettreÀJourAvecUnePrésentationVideEstRefusé()
    {
        var cv = CurriculumVitae.Constituer(Guid.NewGuid(), "Présentation initiale", new[] { "C#" });

        Action action = () => cv.MettreAJour("  ", new[] { "Azure" });

        action.Should().Throw<ArgumentException>();
    }
}
