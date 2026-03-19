namespace PetSearchHome_WEB.Domain.Interfaces
{
    public interface IEmailSender
    {
        Task SendPasswordResetAsync(string email, string resetToken, CancellationToken cancellationToken = default);
    }
}
