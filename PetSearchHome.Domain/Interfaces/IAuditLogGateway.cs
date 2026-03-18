namespace PetSearchHome_WEB.Domain.Interfaces
{
    public interface IAuditLogGateway
    {
        Task RecordAsync(string action, Guid actorId, string context, CancellationToken cancellationToken = default);
    }
}
