namespace PetSearchHome_WEB.Domain.ValueObjects
{
    public readonly record struct Ownership(Guid OwnerId, Role OwnerRole);
}
