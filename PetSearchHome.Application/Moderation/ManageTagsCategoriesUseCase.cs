using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.Policies;

namespace PetSearchHome_WEB.Application.Moderation
{
    public sealed record ManageTagsCategoriesRequest(string Name, bool IsCategory, bool Remove, Guid? Id);

    public class ManageTagsCategoriesUseCase : IUseCase<ManageTagsCategoriesRequest, bool>
    {
        private readonly ITagRepository _tags;
        private readonly ICategoryRepository _categories;

        public ManageTagsCategoriesUseCase(ITagRepository tags, ICategoryRepository categories)
        {
            _tags = tags;
            _categories = categories;
        }

        public async Task<bool> ExecuteAsync(ManageTagsCategoriesRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (!AdminPolicy.IsAdmin(authContext.Role))
            {
                throw new UnauthorizedAccessException("Admin role required.");
            }

            if (request.IsCategory)
            {
                if (request.Remove && request.Id.HasValue)
                {
                    await _categories.RemoveAsync(request.Id.Value, cancellationToken);
                }
                else
                {
                    await _categories.AddAsync(new Category { Name = request.Name }, cancellationToken);
                }
            }
            else
            {
                if (request.Remove && request.Id.HasValue)
                {
                    await _tags.RemoveAsync(request.Id.Value, cancellationToken);
                }
                else
                {
                    await _tags.AddAsync(new Tag { Name = request.Name }, cancellationToken);
                }
            }

            return true;
        }
    }
}
