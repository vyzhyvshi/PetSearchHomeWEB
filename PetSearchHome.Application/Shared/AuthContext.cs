using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Application.Shared
{
    public readonly record struct AuthContext(Guid? UserId, Role Role);
}
