using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.Policies;

namespace PetSearchHome_WEB.Application.Moderation
{
    public sealed record ManageTagsCategoriesRequest(string Name, bool IsCategory, bool Remove, Guid? Id);

    // ЗМІНЕНО: тепер повертає Result<bool>
    public class ManageTagsCategoriesUseCase : IUseCase<ManageTagsCategoriesRequest, Result<bool>>
    {
        private readonly ITagRepository _tags;
        private readonly ICategoryRepository _categories;

        public ManageTagsCategoriesUseCase(ITagRepository tags, ICategoryRepository categories)
        {
            _tags = tags;
            _categories = categories;
        }

        public async Task<Result<bool>> ExecuteAsync(ManageTagsCategoriesRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            // ЗМІНЕНО: замість throw тепер безпечний Result.Failure
            if (!AdminPolicy.IsAdmin(authContext.Role))
            {
                return Result.Failure<bool>("Немає прав доступу. Потрібна роль Адміністратора.");
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

            return Result.Success(true);
        }
    }
}