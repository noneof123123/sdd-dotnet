using Microsoft.AspNetCore.Http;

namespace RealtorApi.Infrastructure.Storage;

public interface IPropertyImageStorage
{
    Task<string> StoreAsync(IFormFile image, CancellationToken cancellationToken);

    Task RemoveAsync(string imageUrl, CancellationToken cancellationToken);
}
