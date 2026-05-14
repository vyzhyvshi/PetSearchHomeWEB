using Microsoft.EntityFrameworkCore;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Infrastructure.Persistence;
// using PetSearchHome_WEB.Infrastructure.Data; 

namespace PetSearchHome_WEB.Infrastructure.Repositories 
{
    public class EfNotificationRepository : INotificationRepository
    {
        private readonly ApplicationDbContext _db;

        public EfNotificationRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<Notification>> GetByRecipientIdAsync(int recipientId, CancellationToken cancellationToken = default)
        {
            return await _db.Notifications
                .Where(n => n.RecipientId == recipientId)
                .OrderByDescending(n => n.CreatedAt) 
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
        {
            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task MarkAsReadAsync(int id, CancellationToken cancellationToken = default)
        {
            var notification = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
            if (notification != null)
            {
                notification.IsRead = true;
                await _db.SaveChangesAsync(cancellationToken);
            }
        }
    }
}