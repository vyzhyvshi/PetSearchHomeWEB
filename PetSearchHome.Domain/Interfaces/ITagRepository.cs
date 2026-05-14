using PetSearchHome_WEB.Domain.Entities;

namespace PetSearchHome_WEB.Domain.Interfaces
{
    public interface ITagRepository
    {
        Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Tag tag, CancellationToken cancellationToken = default);
        Task RemoveAsync(int id, CancellationToken cancellationToken = default);
    }
}
