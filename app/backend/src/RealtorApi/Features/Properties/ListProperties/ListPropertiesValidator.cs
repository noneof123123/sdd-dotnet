using FluentValidation;

namespace RealtorApi.Features.Properties.ListProperties;

public sealed class ListPropertiesValidator : AbstractValidator<ListPropertiesRequest>
{
    public ListPropertiesValidator()
    {
        RuleFor(request => request.Page)
            .Must(page => !page.HasValue || page.Value >= 1)
            .WithMessage("Page debe ser mayor o igual a 1.");

        RuleFor(request => request.PageSize)
            .Must(pageSize => !pageSize.HasValue || pageSize.Value >= 1)
            .WithMessage("PageSize debe ser mayor o igual a 1.");
    }
}
