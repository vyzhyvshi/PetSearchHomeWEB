namespace PetSearchHome_WEB.Domain.Interfaces
{
    public interface IStorageGateway
    {
        Task<string> UploadAsync(string fileName, Stream content, CancellationToken cancellationToken = default);
        Task DeleteAsync(string fileName, CancellationToken cancellationToken = default);
    }
}
