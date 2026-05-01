using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace PetSearchHome.Infrastructure.Gateways;

public class GeocodingOptions
{
 public string BaseUrl { get; set; } = "https://api.geoapify.com";
 public string UserAgent { get; set; } = "PetSearchHome/1.0";
 public string ApiKey { get; set; } = string.Empty;
}

public class ExternalGeocodingClient
{
 private readonly HttpClient _http;
 private readonly GeocodingOptions _options;
 public ExternalGeocodingClient(HttpClient http, IOptions<GeocodingOptions> options)
 {
 _http = http;
 _options = options.Value;
 _http.BaseAddress = new Uri(_options.BaseUrl);
 _http.DefaultRequestHeaders.UserAgent.ParseAdd(_options.UserAgent);
 }

 public async Task<string?> ReverseLookupCityAsync(string query, CancellationToken cancellationToken = default)
 {
 if (string.IsNullOrWhiteSpace(_options.ApiKey)) return null;
 var url = $"/v1/geocode/search?text={Uri.EscapeDataString(query)}&limit=1&lang=uk&apiKey={Uri.EscapeDataString(_options.ApiKey)}";
 using var respMsg = await _http.GetAsync(url, cancellationToken);
 respMsg.EnsureSuccessStatusCode();
 var doc = await respMsg.Content.ReadFromJsonAsync<GeoapifyResponse>(cancellationToken: cancellationToken);
 var feature = doc?.features?.FirstOrDefault();
 return feature?.properties?.formatted;
 }

 public async Task<IReadOnlyList<string>> SearchAsync(string query, CancellationToken cancellationToken = default)
 {
 if (string.IsNullOrWhiteSpace(_options.ApiKey)) return Array.Empty<string>();
 var url = $"/v1/geocode/autocomplete?text={Uri.EscapeDataString(query)}&limit=10&lang=uk&apiKey={Uri.EscapeDataString(_options.ApiKey)}";
 using var respMsg = await _http.GetAsync(url, cancellationToken);
 respMsg.EnsureSuccessStatusCode();
 var doc = await respMsg.Content.ReadFromJsonAsync<GeoapifyResponse>(cancellationToken: cancellationToken);
 if (doc?.features == null) return Array.Empty<string>();
 return doc.features
 .Where(f => f?.properties?.formatted != null)
 .Select(f => f.properties!.formatted!)
 .Distinct()
 .ToList();
 }

 private record GeoapifyResponse(List<Feature>? features);
 private record Feature(GeoProps? properties);
 private record GeoProps(string? formatted);
}
