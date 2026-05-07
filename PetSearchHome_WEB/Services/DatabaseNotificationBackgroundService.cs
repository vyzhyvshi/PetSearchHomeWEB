using Microsoft.EntityFrameworkCore;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;
using PetSearchHome_WEB.Infrastructure.Persistence;

namespace PetSearchHome_WEB.Services;

public sealed class DatabaseNotificationBackgroundService : BackgroundService, IHostedService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan LookbackWindow = TimeSpan.FromSeconds(30);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DatabaseNotificationBackgroundService> _logger;
    private DateTime _lastCheckedUtc = DateTime.UtcNow;

    public DatabaseNotificationBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<DatabaseNotificationBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(PollInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            var fromUtc = _lastCheckedUtc.Subtract(LookbackWindow);
            var toUtc = DateTime.UtcNow;

            try
            {
                await CheckDatabaseEventsAsync(fromUtc, toUtc, stoppingToken);
                _lastCheckedUtc = toUtc;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to process background notification events.");
            }

            if (!await timer.WaitForNextTickAsync(stoppingToken))
            {
                break;
            }
        }
    }

    private async Task CheckDatabaseEventsAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var notificationGateway = scope.ServiceProvider.GetRequiredService<INotificationGateway>();

        var adminUserIds = await db.Users
            .AsNoTracking()
            .Where(u => u.Role == Role.Admin && u.DeletedAt == null && u.IsActive)
            .Select(u => u.UserId)
            .ToListAsync(cancellationToken);

        await NotifyAdminsAboutPendingListingsAsync(db, notificationGateway, adminUserIds, fromUtc, toUtc, cancellationToken);
        await NotifyOwnersAboutNewFavoritesAsync(db, notificationGateway, fromUtc, toUtc, cancellationToken);
        await NotifyRecipientsAboutChatMessagesAsync(db, notificationGateway, fromUtc, toUtc, cancellationToken);
    }

    private static async Task NotifyAdminsAboutPendingListingsAsync(
        ApplicationDbContext db,
        INotificationGateway notificationGateway,
        IReadOnlyCollection<int> adminUserIds,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken)
    {
        if (adminUserIds.Count == 0)
        {
            return;
        }

        var listings = await db.Listings
            .AsNoTracking()
            .Where(l => l.Status == ListingStatus.PendingModeration)
            .Where(l => l.CreatedAt > fromUtc && l.CreatedAt <= toUtc)
            .Select(l => new { l.ListingId, l.Title })
            .ToListAsync(cancellationToken);

        foreach (var listing in listings)
        {
            foreach (var adminUserId in adminUserIds)
            {
                var recipientId = ToDomainId(adminUserId);
                var message = $"Нове оголошення очікує модерації: {listing.Title} (#{listing.ListingId}).";
                await NotifyIfMissingAsync(db, notificationGateway, recipientId, message, cancellationToken);
            }
        }
    }

    private static async Task NotifyOwnersAboutNewFavoritesAsync(
        ApplicationDbContext db,
        INotificationGateway notificationGateway,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken)
    {
        var favorites = await db.Favorites
            .AsNoTracking()
            .Include(f => f.Listing)
            .Where(f => f.CreatedAt > fromUtc && f.CreatedAt <= toUtc)
            .Where(f => f.UserId != f.Listing.UserId)
            .Select(f => new
            {
                f.FavoriteId,
                OwnerId = f.Listing.UserId,
                f.Listing.Title
            })
            .ToListAsync(cancellationToken);

        foreach (var favorite in favorites)
        {
            var message = $"Ваше оголошення додали в обране: {favorite.Title} (подія #{favorite.FavoriteId}).";
            await NotifyIfMissingAsync(db, notificationGateway, ToDomainId(favorite.OwnerId), message, cancellationToken);
        }
    }

    private static async Task NotifyRecipientsAboutChatMessagesAsync(
        ApplicationDbContext db,
        INotificationGateway notificationGateway,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken)
    {
        var messages = await db.ChatMessages
            .AsNoTracking()
            .Include(m => m.Conversation)
            .Where(m => m.SentAt > fromUtc && m.SentAt <= toUtc)
            .Select(m => new
            {
                m.MessageId,
                m.SenderId,
                m.Conversation.UserAId,
                m.Conversation.UserBId
            })
            .ToListAsync(cancellationToken);

        foreach (var chatMessage in messages)
        {
            var recipientUserId = chatMessage.SenderId == chatMessage.UserAId
                ? chatMessage.UserBId
                : chatMessage.UserAId;
            var message = $"У вас нове повідомлення в чаті (#{chatMessage.MessageId}).";
            await NotifyIfMissingAsync(db, notificationGateway, ToDomainId(recipientUserId), message, cancellationToken);
        }
    }

    private static async Task NotifyIfMissingAsync(
        ApplicationDbContext db,
        INotificationGateway notificationGateway,
        Guid recipientId,
        string message,
        CancellationToken cancellationToken)
    {
        var exists = await db.Notifications
            .AsNoTracking()
            .AnyAsync(n => n.RecipientId == recipientId && n.Message == message, cancellationToken);

        if (exists)
        {
            return;
        }

        await notificationGateway.NotifyAsync(recipientId, message, cancellationToken);
    }

    private static Guid ToDomainId(int value)
    {
        Span<byte> bytes = stackalloc byte[16];
        bytes.Clear();
        BitConverter.TryWriteBytes(bytes, value);
        return new Guid(bytes);
    }
}
