using FluentValidation;

namespace CVTech.Modules.GestionIdentite.Application.Features.BloquerCompte;

public sealed class BloquerCompteValidator : AbstractValidator<BloquerCompteCommand>
{
    public BloquerCompteValidator()
    {
        RuleFor(c => c.AppelantId).NotEmpty();
        RuleFor(c => c.CompteCibleId).NotEmpty();
    }
}
