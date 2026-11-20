using FluentValidation;
using RealtorApi.Domain.Properties;

namespace RealtorApi.Features.Properties.ChangePropertyStatus;

public sealed class ChangePropertyStatusValidator : AbstractValidator<ChangePropertyStatusRequest>
{
    public ChangePropertyStatusValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty()
            .WithMessage("El identificador de la propiedad es obligatorio.")
            .Must(value => Guid.TryParse(value, out _))
            .WithMessage("El identificador de la propiedad debe ser un GUID válido.");

        RuleFor(request => request.Body)
            .NotNull()
            .WithMessage("El cuerpo de la solicitud es obligatorio.")
            .OverridePropertyName(nameof(ChangePropertyStatusBody.Status));

        RuleFor(request => request.Body!.Status)
            .NotEmpty()
            .WithMessage("El estado es obligatorio.")
            .Must(IsAllowedStatus)
            .WithMessage("El estado debe ser Available, Rented o Maintenance.")
            .When(request => request.Body is not null)
            .OverridePropertyName(nameof(ChangePropertyStatusBody.Status));
    }

    private static bool IsAllowedStatus(string? status) => status is
        nameof(PropertyStatus.Available) or
        nameof(PropertyStatus.Rented) or
        nameof(PropertyStatus.Maintenance);
}
