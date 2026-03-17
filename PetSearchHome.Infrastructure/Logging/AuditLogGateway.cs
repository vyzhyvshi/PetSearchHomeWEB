using Microsoft.Extensions.Logging;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Infrastructure.Logging
{
    public class AuditLogGateway : IAuditLogGateway
    {
        private readonly ILogger<AuditLogGateway> _logger;

        public AuditLogGateway(ILogger<AuditLogGateway> logger)
        {
            _logger = logger;
        }

        public Task RecordAsync(string action, Guid actorId, string context, CancellationToken cancellationToken = default)
        {
            // Форматуємо повідомлення аудиту. 
            // В майбутньому тут можна додати код для збереження події в таблицю БД (наприклад, AuditLogs)
            _logger.LogInformation("Аудит дії: Дія='{Action}', Користувач='{ActorId}', Контекст='{Context}'", 
                action, actorId, context);
            
            return Task.CompletedTask;
        }
    }
}