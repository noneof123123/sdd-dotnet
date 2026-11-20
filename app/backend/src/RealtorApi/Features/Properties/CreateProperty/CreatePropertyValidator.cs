using FluentValidation;
using Microsoft.AspNetCore.Http;
using RealtorApi.Domain.Properties;

namespace RealtorApi.Features.Properties.CreateProperty;

public sealed class CreatePropertyValidator : AbstractValidator<CreatePropertyRequest>
{
    private const int MaxImageLength = 5 * 1024 * 1024;
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly string[] AllowedExtensions = [".png", ".jpg", ".jpeg"];
    private static readonly string[] AllowedMimeTypes = ["image/png", "image/jpeg", "image/jpg"];

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreatePropertyValidator(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;

        RuleFor(request => request.Title).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Description).NotEmpty().MaximumLength(4000);
        RuleFor(request => request.Address).NotEmpty().MaximumLength(500);
        RuleFor(request => request.Price).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Status).IsInEnum();
        RuleFor(request => request.BedroomCount).GreaterThanOrEqualTo(0);
        RuleFor(request => request.BathroomCount).GreaterThanOrEqualTo(0);
        RuleFor(request => request.AreaSquareMeters).GreaterThanOrEqualTo(0);

        RuleFor(request => request)
            .Must(_ => HttpContextIsAvailable())
            .WithMessage("No se pudo inspeccionar la solicitud multipart.")
            .OverridePropertyName(nameof(CreatePropertyRequest.Image));

        RuleFor(request => request)
            .Must(_ => HasValidMultipartContentType())
            .WithMessage("La solicitud debe usar multipart/form-data.")
            .OverridePropertyName(nameof(CreatePropertyRequest.Image));

        RuleFor(request => request)
            .Must(_ => HasValidImagePartCount())
            .WithMessage("Solo puede existir una parte image y debe ser un archivo.")
            .OverridePropertyName(nameof(CreatePropertyRequest.Image));

        RuleFor(request => request.Image)
            .MustAsync(IsValidImageAsync)
            .When(request => request.Image is not null)
            .WithMessage("La imagen debe ser PNG o JPG/JPEG válida de hasta 5 × 1024 × 1024 bytes.")
            .OverridePropertyName(nameof(CreatePropertyRequest.Image));
    }

    private bool HttpContextIsAvailable() => _httpContextAccessor.HttpContext is not null;

    private bool HasValidMultipartContentType()
    {
        var contentType = _httpContextAccessor.HttpContext?.Request.ContentType;
        return !string.IsNullOrWhiteSpace(contentType)
            && contentType.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase);
    }

    private bool HasValidImagePartCount()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
            return false;

        var fileCount = httpContext.Request.Form.Files.GetFiles("image").Count;
        var nonFileCount = httpContext.Request.Form.Keys
            .Where(key => string.Equals(key, "image", StringComparison.OrdinalIgnoreCase))
            .Sum(key => httpContext.Request.Form[key].Count);

        return nonFileCount == 0 && fileCount <= 1;
    }

    private static async Task<bool> IsValidImageAsync(
        IFormFile? image,
        CancellationToken cancellationToken)
    {
        if (image is null || image.Length <= 0 || image.Length > MaxImageLength)
            return false;

        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        var mimeType = image.ContentType?.ToLowerInvariant() ?? string.Empty;
        if (!AllowedExtensions.Contains(extension) || !AllowedMimeTypes.Contains(mimeType))
            return false;

        if (extension == ".png" && mimeType != "image/png")
            return false;
        if (extension is ".jpg" or ".jpeg" && mimeType is not ("image/jpeg" or "image/jpg"))
            return false;

        byte[] header = new byte[PngSignature.Length];
        await using var stream = image.OpenReadStream();
        var read = await stream.ReadAsync(header.AsMemory(0, header.Length), cancellationToken);
        stream.Position = 0;

        var isPng = read >= PngSignature.Length && header.AsSpan(0, PngSignature.Length).SequenceEqual(PngSignature);
        var isJpeg = read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;

        return extension == ".png" ? isPng : isJpeg;
    }
}
