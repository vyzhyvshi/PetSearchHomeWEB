namespace PetSearchHome_WEB.Domain.Interfaces
{
    public interface IAuditLogGateway
    {
        Task RecordAsync(string action, int actorId, string context, CancellationToken cancellationToken = default);
    }
}
