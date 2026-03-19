using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.Policies;

namespace PetSearchHome_WEB.Application.Moderation
{
    public sealed record GetOpenComplaintsRequest();

    public class GetOpenComplaintsUseCase : IUseCase<GetOpenComplaintsRequest, IReadOnlyList<Complaint>>
    {
        private readonly IComplaintRepository _complaints;

        public GetOpenComplaintsUseCase(IComplaintRepository complaints)
        {
            _complaints = complaints;
        }

        public async Task<IReadOnlyList<Complaint>> ExecuteAsync(GetOpenComplaintsRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (!AdminPolicy.IsAdmin(authContext.Role))
            {
                throw new UnauthorizedAccessException("Admin role required.");
            }

            // Припускаємо, що у твоєму IComplaintRepository є метод для отримання відкритих скарг
            return await _complaints.ListOpenAsync(cancellationToken);
        }
    }
}