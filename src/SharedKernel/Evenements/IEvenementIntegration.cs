using MediatR;

namespace CVTech.SharedKernel.Evenements;

/// <summary>
/// Marqueur d'un événement d'intégration publié sur le bus interne en mémoire
/// (ADR 0003). Hérite de <see cref="INotification"/> : la diffusion inter-modules
/// s'appuie sur le mécanisme de notifications de MediatR.
/// </summary>
public interface IEvenementIntegration : INotification;
