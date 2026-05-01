using PetSearchHome_WEB.Application.Shared;

namespace PetSearchHome.Infrastructure.Gateways;

public class GeocodingServiceAdapter : IGeocodingService
{
 private readonly ExternalGeocodingClient _client;
 public GeocodingServiceAdapter(ExternalGeocodingClient client)
 {
 _client = client;
 }

 public async Task<Result<(string city, string district)>> ResolveLocationAsync(string query, CancellationToken cancellationToken = default)
 {
 var display = await _client.ReverseLookupCityAsync(query, cancellationToken);
 if (string.IsNullOrWhiteSpace(display)) return Result.Failure<(string, string)>("NotFound");
 var parts = display.Split(',',3, System.StringSplitOptions.TrimEntries | System.StringSplitOptions.RemoveEmptyEntries);
 var tuple = parts.Length switch
 {
 >=2 => (parts[0], parts[1]),
1 => (parts[0], string.Empty),
 _ => (display, string.Empty)
 };
 return Result.Success(tuple);
 }
}
