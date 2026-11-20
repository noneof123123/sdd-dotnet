using Microsoft.AspNetCore.Mvc;
using RealtorApi.Domain.Properties;

namespace RealtorApi.Features.Properties.CreateProperty;

public sealed record CreatePropertyRequest
{
    [FromForm(Name = "title")]
    public string Title { get; init; } = string.Empty;

    [FromForm(Name = "description")]
    public string Description { get; init; } = string.Empty;

    [FromForm(Name = "address")]
    public string Address { get; init; } = string.Empty;

    [FromForm(Name = "price")]
    public decimal Price { get; init; }

    [FromForm(Name = "status")]
    public PropertyStatus Status { get; init; }

    [FromForm(Name = "bedroomCount")]
    public int BedroomCount { get; init; }

    [FromForm(Name = "bathroomCount")]
    public int BathroomCount { get; init; }

    [FromForm(Name = "areaSquareMeters")]
    public decimal AreaSquareMeters { get; init; }

    [FromForm(Name = "image")]
    public IFormFile? Image { get; init; }
}
