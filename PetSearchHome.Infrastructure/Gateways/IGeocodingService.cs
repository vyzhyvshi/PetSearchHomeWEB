using PetSearchHome_WEB.Application.Shared;

namespace PetSearchHome.Infrastructure.Gateways;

public interface IGeocodingService
{
 Task<Result<(string city, string district)>> ResolveLocationAsync(string query, CancellationToken cancellationToken = default);
}
