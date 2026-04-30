using System.Threading;
using System.Threading.Tasks;

namespace PetSearchHome_WEB.Application.Services
{
 public interface IGeocodingService
 {
 Task<(string city, string district)> ResolveLocationAsync(string query, CancellationToken cancellationToken = default);
 }
}
