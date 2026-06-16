using FluentValidation;

namespace CVTech.Modules.GestionIdentite.Application.Features.Authentifier;

public sealed class AuthentifierValidator : AbstractValidator<AuthentifierCommand>
{
    public AuthentifierValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
        RuleFor(c => c.MotDePasse).NotEmpty();
    }
}
