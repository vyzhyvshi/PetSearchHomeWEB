using Microsoft.EntityFrameworkCore;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Infrastructure.Persistence;
using PetSearchHome_WEB.Infrastructure.Persistence.Entities;

namespace PetSearchHome_WEB.Infrastructure.Repositories;

public class EfChatRepository : IChatRepository
{
    private readonly ApplicationDbContext _db;

    public EfChatRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ChatConversation?> GetConversationAsync(Guid userId, Guid otherUserId, CancellationToken cancellationToken = default)
    {
        var (userAId, userBId) = NormalizePair(userId, otherUserId);
        var entity = await _db.ChatConversations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserAId == userAId && c.UserBId == userBId, cancellationToken);

        return entity is null ? null : Map(entity);
    }

    public async Task<ChatConversation?> GetConversationByIdAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        var id = FromDomainId(conversationId);
        var entity = await _db.ChatConversations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ConversationId == id, cancellationToken);

        return entity is null ? null : Map(entity);
    }

    public async Task<ChatConversation> CreateConversationAsync(Guid userId, Guid otherUserId, CancellationToken cancellationToken = default)
    {
        var (userAId, userBId) = NormalizePair(userId, otherUserId);
        var entity = new ChatConversationEntity
        {
            UserAId = userAId,
            UserBId = userBId,
            CreatedAt = DateTime.UtcNow
        };

        await _db.ChatConversations.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return Map(entity);
    }

    public async Task<IReadOnlyList<ChatConversation>> ListConversationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var id = FromDomainId(userId);
        var entities = await _db.ChatConversations
            .AsNoTracking()
            .Where(c => c.UserAId == id || c.UserBId == id)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<ChatMessage>> ListMessagesAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        var id = FromDomainId(conversationId);
        var entities = await _db.ChatMessages
            .AsNoTracking()
            .Where(m => m.ConversationId == id)
            .OrderBy(m => m.SentAt)
            .ToListAsync(cancellationToken);

        return entities.Select(Map).ToList();
    }

    public async Task<ChatMessage?> GetLastMessageAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        var id = FromDomainId(conversationId);
        var entity = await _db.ChatMessages
            .AsNoTracking()
            .Where(m => m.ConversationId == id)
            .OrderByDescending(m => m.SentAt)
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null ? null : Map(entity);
    }

    public async Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken = default)
    {
        var entity = new ChatMessageEntity
        {
            ConversationId = FromDomainId(message.ConversationId),
            SenderId = FromDomainId(message.SenderId),
            Content = message.Content,
            ImageUrl = message.ImageUrl,
            SentAt = message.SentAt.UtcDateTime
        };

        await _db.ChatMessages.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<ChatMessage?> GetMessageByIdAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        var id = FromDomainId(messageId);
        var entity = await _db.ChatMessages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.MessageId == id, cancellationToken);

        return entity is null ? null : Map(entity);
    }

    public async Task DeleteMessageAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        var id = FromDomainId(messageId);
        var entity = await _db.ChatMessages.FirstOrDefaultAsync(m => m.MessageId == id, cancellationToken);
        if (entity is null)
        {
            return;
        }

        _db.ChatMessages.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IsBlockedAsync(Guid blockerId, Guid blockedId, CancellationToken cancellationToken = default)
    {
        var blocker = FromDomainId(blockerId);
        var blocked = FromDomainId(blockedId);
        return await _db.ChatBlocks
            .AsNoTracking()
            .AnyAsync(b => b.BlockerId == blocker && b.BlockedId == blocked, cancellationToken);
    }

    public async Task SetBlockedAsync(Guid blockerId, Guid blockedId, bool isBlocked, CancellationToken cancellationToken = default)
    {
        var blocker = FromDomainId(blockerId);
        var blocked = FromDomainId(blockedId);

        var entity = await _db.ChatBlocks
            .FirstOrDefaultAsync(b => b.BlockerId == blocker && b.BlockedId == blocked, cancellationToken);

        if (isBlocked)
        {
            if (entity is null)
            {
                await _db.ChatBlocks.AddAsync(new ChatBlockEntity
                {
                    BlockerId = blocker,
                    BlockedId = blocked,
                    CreatedAt = DateTime.UtcNow
                }, cancellationToken);
            }
        }
        else if (entity is not null)
        {
            _db.ChatBlocks.Remove(entity);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private static (int UserAId, int UserBId) NormalizePair(Guid userId, Guid otherUserId)
    {
        var userAId = FromDomainId(userId);
        var userBId = FromDomainId(otherUserId);
        return userAId <= userBId ? (userAId, userBId) : (userBId, userAId);
    }

    private static ChatConversation Map(ChatConversationEntity entity) => new()
    {
        Id = ToDomainId(entity.ConversationId),
        UserAId = ToDomainId(entity.UserAId),
        UserBId = ToDomainId(entity.UserBId),
        CreatedAt = new DateTimeOffset(DateTime.SpecifyKind(entity.CreatedAt, DateTimeKind.Utc))
    };

    private static ChatMessage Map(ChatMessageEntity entity) => new()
    {
        Id = ToDomainId(entity.MessageId),
        ConversationId = ToDomainId(entity.ConversationId),
        SenderId = ToDomainId(entity.SenderId),
        Content = entity.Content,
        ImageUrl = entity.ImageUrl,
        SentAt = new DateTimeOffset(DateTime.SpecifyKind(entity.SentAt, DateTimeKind.Utc))
    };

    private static Guid ToDomainId(int value)
    {
        Span<byte> bytes = stackalloc byte[16];
        bytes.Clear();
        BitConverter.TryWriteBytes(bytes, value);
        return new Guid(bytes);
    }

    private static int FromDomainId(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new InvalidOperationException("Identifier is not set.");
        }

        var bytes = id.ToByteArray();
        return BitConverter.ToInt32(bytes, 0);
    }
}
