namespace PetSearchHome_WEB.Domain.Entities
{
    public class Tag
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Name { get; init; } = string.Empty;
    }
}
