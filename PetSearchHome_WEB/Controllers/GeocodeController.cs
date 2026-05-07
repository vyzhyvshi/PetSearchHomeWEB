using Microsoft.AspNetCore.Mvc;
using PetSearchHome.Infrastructure.Gateways;

namespace PetSearchHome_WEB.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GeocodeController : ControllerBase
{
    private readonly ExternalGeocodingClient _client;
    public GeocodeController(ExternalGeocodingClient client)
    {
        _client = client;
    }

    [HttpGet("autocomplete")]
    public async Task<IActionResult> Autocomplete(string q, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q)) return Ok(Array.Empty<string>());

        if (cancellationToken.IsCancellationRequested)
        {
            return Ok(Array.Empty<string>());
        }

        try
        {
            var list = await _client.SearchAsync(q, cancellationToken);
            return Ok(list);
        }
        catch (OperationCanceledException)
        {
            return Ok(Array.Empty<string>());
        }
        catch (HttpRequestException)
        {
            return Ok(Array.Empty<string>());
        }
    }
}
