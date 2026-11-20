using FluentValidation;

namespace RealtorApi.Features.Properties.GetPropertyById;

public sealed class GetPropertyByIdValidator : AbstractValidator<GetPropertyByIdRequest>
{
    public GetPropertyByIdValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty()
            .WithMessage("El identificador de la propiedad es obligatorio.");
    }
}
