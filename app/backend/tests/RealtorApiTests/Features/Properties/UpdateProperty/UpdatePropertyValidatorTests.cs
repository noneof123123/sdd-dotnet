using Microsoft.AspNetCore.Http;
using RealtorApi.Domain.Properties;
using RealtorApi.Features.Properties.UpdateProperty;

namespace RealtorApiTests.Features.Properties.UpdateProperty;

public sealed class UpdatePropertyValidatorTests
{
    [Fact]
    public async Task Validate_WithValidIdFieldsAndNoImage_ReturnsValid()
    {
        var context = CreateContext();
        var validator = new UpdatePropertyValidator(new HttpContextAccessor { HttpContext = context });

        var result = await validator.ValidateAsync(CreateRequest());

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithEmptyIdAndInvalidFields_ReturnsErrors()
    {
        var context = CreateContext();
        var validator = new UpdatePropertyValidator(new HttpContextAccessor { HttpContext = context });
        var request = CreateRequest() with
        {
            Id = Guid.Empty,
            Title = string.Empty,
            Description = new string('D', 4001),
            Address = new string('A', 501),
            Price = -1m,
            Status = (PropertyStatus)99,
            BedroomCount = -1,
            BathroomCount = -1,
            AreaSquareMeters = -1m
        };

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdatePropertyRequest.Id));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdatePropertyRequest.Title));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdatePropertyRequest.Description));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdatePropertyRequest.Address));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdatePropertyRequest.Price));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdatePropertyRequest.Status));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdatePropertyRequest.BedroomCount));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdatePropertyRequest.BathroomCount));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdatePropertyRequest.AreaSquareMeters));
    }

    [Theory]
    [InlineData("image/gif", "photo.gif", "GIF89a")]
    [InlineData("application/pdf", "document.pdf", "%PDF-")]
    public async Task Validate_WithUnsupportedMimeOrExtension_ReturnsError(string mime, string fileName, string text)
    {
        var file = CreateFile("image", fileName, mime, System.Text.Encoding.ASCII.GetBytes(text));
        var validator = new UpdatePropertyValidator(new HttpContextAccessor { HttpContext = CreateContext(file) });

        var result = await validator.ValidateAsync(CreateRequest() with { Image = file });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdatePropertyRequest.Image));
    }

    [Fact]
    public async Task Validate_WithContentMismatchingSignature_ReturnsError()
    {
        byte[] jpegSignature = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10];
        var file = CreateFile("image", "photo.png", "image/png", jpegSignature);
        var validator = new UpdatePropertyValidator(new HttpContextAccessor { HttpContext = CreateContext(file) });

        var result = await validator.ValidateAsync(CreateRequest() with { Image = file });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdatePropertyRequest.Image));
    }

    [Fact]
    public async Task Validate_WithImageAboveBinaryLimit_ReturnsError()
    {
        var bytes = new byte[(5 * 1024 * 1024) + 1];
        bytes[0] = 0x89;
        bytes[1] = 0x50;
        bytes[2] = 0x4E;
        bytes[3] = 0x47;
        bytes[4] = 0x0D;
        bytes[5] = 0x0A;
        bytes[6] = 0x1A;
        bytes[7] = 0x0A;
        var file = CreateFile("image", "large.png", "image/png", bytes);
        var validator = new UpdatePropertyValidator(new HttpContextAccessor { HttpContext = CreateContext(file) });

        var result = await validator.ValidateAsync(CreateRequest() with { Image = file });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdatePropertyRequest.Image));
    }

    [Theory]
    [InlineData("image/png", "photo.png")]
    [InlineData("image/jpeg", "photo.jpg")]
    [InlineData("image/jpeg", "photo.jpeg")]
    public async Task Validate_WithValidPngOrJpeg_ReturnsValid(string mime, string fileName)
    {
        byte[] bytes = mime == "image/png"
            ? [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x01]
            : [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x01];
        var file = CreateFile("image", fileName, mime, bytes);
        var validator = new UpdatePropertyValidator(new HttpContextAccessor { HttpContext = CreateContext(file) });

        var result = await validator.ValidateAsync(CreateRequest() with { Image = file });

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithTwoImageParts_ReturnsError()
    {
        byte[] png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        var first = CreateFile("image", "first.png", "image/png", png);
        var second = CreateFile("image", "second.png", "image/png", png);
        var validator = new UpdatePropertyValidator(new HttpContextAccessor { HttpContext = CreateContext(first, second) });

        var result = await validator.ValidateAsync(CreateRequest() with { Image = first });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdatePropertyRequest.Image));
    }

    [Fact]
    public async Task Validate_WithNonFileImagePart_ReturnsError()
    {
        var context = CreateContext();
        context.Request.Form = new FormCollection(
            new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["image"] = "contenido-no-archivo"
            },
            new FormFileCollection());
        var validator = new UpdatePropertyValidator(new HttpContextAccessor { HttpContext = context });

        var result = await validator.ValidateAsync(CreateRequest());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdatePropertyRequest.Image));
    }

    [Fact]
    public async Task Validate_WithNonMultipartRequest_ReturnsError()
    {
        var context = CreateContext();
        context.Request.ContentType = "application/json";
        var validator = new UpdatePropertyValidator(new HttpContextAccessor { HttpContext = context });

        var result = await validator.ValidateAsync(CreateRequest());

        Assert.False(result.IsValid);
    }

    private static UpdatePropertyRequest CreateRequest() => new()
    {
        Id = Guid.NewGuid(),
        Title = "Casa de prueba",
        Description = "Propiedad actualizada para validación.",
        Address = "Calle Central 123",
        Price = 250000.00m,
        Status = PropertyStatus.Available,
        BedroomCount = 3,
        BathroomCount = 2,
        AreaSquareMeters = 140.50m
    };

    private static DefaultHttpContext CreateContext(params IFormFile[] files)
    {
        var context = new DefaultHttpContext
        {
            Request =
            {
                ContentType = "multipart/form-data; boundary=realtor-update"
            }
        };
        var fileCollection = new FormFileCollection();
        foreach (var file in files)
            fileCollection.Add(file);
        context.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), fileCollection);
        return context;
    }

    private static FormFile CreateFile(string name, string fileName, string contentType, byte[] bytes) =>
        new(new MemoryStream(bytes), 0, bytes.Length, name, fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
}
