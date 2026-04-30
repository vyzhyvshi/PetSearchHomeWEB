using Microsoft.Extensions.Logging;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Infrastructure.Repositories;

public class ConsoleEmailSender : IEmailSender
{
    private readonly ILogger<ConsoleEmailSender> _logger;

    public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendPasswordResetAsync(string email, string resetToken, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Password reset token for {Email}: {ResetToken}", email, resetToken);
        return Task.CompletedTask;
    }
}
