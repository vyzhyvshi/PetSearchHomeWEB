namespace PetSearchHome_WEB.Application.Shared
{
    public interface IUseCase<in TRequest, TResponse>
    {
        Task<TResponse> ExecuteAsync(TRequest request, AuthContext authContext, CancellationToken cancellationToken = default);
    }
}
