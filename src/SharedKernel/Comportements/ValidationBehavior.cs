using FluentValidation;
using MediatR;

namespace CVTech.SharedKernel.Comportements;

/// <summary>
/// Pipeline MediatR partagé : exécute les validateurs FluentValidation d'une
/// requête avant son handler. Enregistré une seule fois globalement (composition
/// root de l'API) pour s'appliquer à tous les modules.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validateurs)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (validateurs.Any())
        {
            var contexte = new ValidationContext<TRequest>(request);
            var echecs = validateurs
                .Select(v => v.Validate(contexte))
                .SelectMany(r => r.Errors)
                .Where(e => e is not null)
                .ToList();

            if (echecs.Count != 0)
                throw new ValidationException(echecs);
        }

        return await next(ct);
    }
}
