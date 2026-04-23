using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;
using PetSearchHome_WEB.Infrastructure.Persistence;
using PetSearchHome_WEB.Infrastructure.Persistence.Entities;

namespace PetSearchHome_WEB.Infrastructure.Repositories;

public class EfListingRepository : IListingRepository, ISearchGateway
{
    private readonly ApplicationDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;

    public EfListingRepository(ApplicationDbContext db, IMemoryCache cache, IConfiguration configuration)
    {
        _db = db;
        _cache = cache;
        _configuration = configuration;
    }

    public async Task<IReadOnlyList<PetListing>> GetFeaturedAsync(int take, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"featured:{take}";

        if (_cache.TryGetValue<IReadOnlyList<PetListing>>(cacheKey, out var cached))
        {
            return cached!;
        }

        var entities = await QueryBase()
            .Where(l => l.Status == ListingStatus.Published)
            .OrderByDescending(l => l.IsUrgent)
            .ThenByDescending(l => l.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        var result = entities.Select(MapToDomain).ToList();

        // read cache duration from configuration (seconds)
        var seconds =300; // default
        var configured = _configuration["CacheSettings:FeaturedSeconds"];
        if (!string.IsNullOrWhiteSpace(configured) && int.TryParse(configured, out var parsed))
        {
            seconds = parsed;
        }

        var cacheEntryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(seconds)
        };

        _cache.Set(cacheKey, result as IReadOnlyList<PetListing>, cacheEntryOptions);

        return result;
    }

    public async Task<PetListing?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await QueryBase()
            .FirstOrDefaultAsync(l => l.DomainId == id, cancellationToken);

        return entity is null ? null : MapToDomain(entity);
    }

    public async Task<IReadOnlyList<PetListing>> ListByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        var userId = FromDomainGuid(ownerId);
        var entities = await QueryBase()
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToList();
    }

    public async Task AddAsync(PetListing listing, CancellationToken cancellationToken = default)
    {
        var (city, district) = SplitLocation(listing.Location);
        var entity = new ListingEntity
        {
            DomainId = listing.Id == Guid.Empty ? Guid.NewGuid() : listing.Id,
            UserId = FromDomainGuid(listing.OwnerId),
            Title = string.IsNullOrWhiteSpace(listing.Title) ? listing.AnimalType : listing.Title,
            AnimalType = listing.AnimalType,
            Location = string.IsNullOrWhiteSpace(listing.Location) ? city : listing.Location,
            IsUrgent = listing.IsUrgent,
            Breed = string.IsNullOrWhiteSpace(listing.Title) ? listing.AnimalType : listing.Title,
            AgeMonths =0,
            Sex = Sex.Unknown,
            Size = PetSize.Unknown,
            Color = "Not specified",
            City = city,
            District = district,
            Description = listing.Description ?? string.Empty,
            Status = listing.Status,
            CreatedAt = listing.ListedAt.UtcDateTime
        };

        foreach (var (url, index) in listing.PhotoUrls
                     .Where(static url => !string.IsNullOrWhiteSpace(url))
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .Select((url, index) => (url.Trim(), index)))
        {
            entity.Photos.Add(new PhotoEntity
            {
                Url = url,
                IsPrimary = index ==0
            });
        }

        await _db.Listings.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        // invalidate featured cache because new listing may affect it
        var cacheKeyPrefix = "featured:";
        // naive approach: remove keys for small takes
        for (int t =1; t <=12; t++)
        {
            _cache.Remove($"{cacheKeyPrefix}{t}");
        }
    }

    public async Task UpdateAsync(PetListing listing, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Listings
            .Include(l => l.Photos)
            .FirstOrDefaultAsync(l => l.DomainId == listing.Id, cancellationToken);
        if (entity is null)
        {
            return;
        }

        var (city, district) = SplitLocation(listing.Location);
        entity.Title = listing.Title;
        entity.AnimalType = listing.AnimalType;
        entity.Location = listing.Location;
        entity.IsUrgent = listing.IsUrgent;
        entity.Breed = listing.Title;
        entity.City = city;
        entity.District = district;
        entity.Description = listing.Description ?? string.Empty;
        entity.Status = listing.Status;
        entity.Photos.Clear();

        foreach (var (url, index) in listing.PhotoUrls
                     .Where(static url => !string.IsNullOrWhiteSpace(url))
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .Select((url, index) => (url.Trim(), index)))
        {
            entity.Photos.Add(new PhotoEntity
            {
                Url = url,
                IsPrimary = index ==0
            });
        }

        await _db.SaveChangesAsync(cancellationToken);

        // invalidate featured cache
        for (int t =1; t <=12; t++)
        {
            _cache.Remove($"featured:{t}");
        }
    }

    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Listings.FirstOrDefaultAsync(l => l.DomainId == id, cancellationToken);
        if (entity is null)
        {
            return;
        }

        _db.Listings.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);

        // invalidate featured cache
        for (int t =1; t <=12; t++)
        {
            _cache.Remove($"featured:{t}");
        }
    }

    public async Task<IReadOnlyList<PetListing>> ListByStatusAsync(ListingStatus status, CancellationToken cancellationToken = default)
    {
        var entities = await QueryBase()
            .Where(l => l.Status == status)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToList();
    }

    public async Task<IReadOnlyList<PetListing>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var list = ids.Distinct().ToArray();
        if (list.Length ==0)
        {
            return Array.Empty<PetListing>();
        }

        var entities = await QueryBase()
            .Where(l => list.Contains(l.DomainId))
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToList();
    }

    public async Task<IReadOnlyList<PetListing>> SearchAsync(SearchFilters filters, CancellationToken cancellationToken = default)
    {
        var query = QueryBase().Where(l => l.Status == ListingStatus.Published);

        if (!string.IsNullOrWhiteSpace(filters.SearchQuery))
        {
            var term = $"%{filters.SearchQuery.Trim()}%";
            query = query.Where(l => EF.Functions.ILike(l.Title, term) || EF.Functions.ILike(l.Description, term));
        }

        if (!string.IsNullOrWhiteSpace(filters.AnimalType))
        {
            var animalType = filters.AnimalType.Trim();
            query = query.Where(l => EF.Functions.ILike(l.AnimalType, animalType));
        }

        if (!string.IsNullOrWhiteSpace(filters.Location))
        {
            var location = $"%{filters.Location.Trim()}%";
            query = query.Where(l => EF.Functions.ILike(l.Location, location));
        }

        if (filters.IsUrgent.HasValue)
        {
            query = query.Where(l => l.IsUrgent == filters.IsUrgent.Value);
        }

        var entities = await query
            .OrderByDescending(l => l.IsUrgent)
            .ThenByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToList();
    }

    private IQueryable<ListingEntity> QueryBase() =>
        _db.Listings
            .AsNoTracking()
            .Include(l => l.Photos);

    private static PetListing MapToDomain(ListingEntity entity)
    {
        var createdAt = DateTime.SpecifyKind(entity.CreatedAt, DateTimeKind.Utc);
        var photoUrls = entity.Photos
            ?.OrderByDescending(p => p.IsPrimary)
            .ThenBy(p => p.PhotoId)
            .Select(p => p.Url)
            .ToList() ?? new List<string>();

        return new PetListing
        {
            Id = entity.DomainId,
            OwnerId = ToDomainGuid(entity.UserId),
            OwnerRole = entity.User?.Role ?? Role.Person,
            Title = entity.Title,
            AnimalType = entity.AnimalType,
            Location = entity.Location,
            Description = entity.Description,
            IsUrgent = entity.IsUrgent,
            Status = entity.Status,
            ListedAt = new DateTimeOffset(createdAt),
            PhotoUrls = photoUrls
        };
    }

    private static (string city, string district) SplitLocation(string location)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            return ("Unknown", string.Empty);
        }

        var parts = location.Split(',',2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
0 => (location, string.Empty),
1 => (parts[0], string.Empty),
 _ => (parts[0], parts[1])
 };
 }

    private static Guid ToDomainGuid(int value)
    {
        Span<byte> buffer = stackalloc byte[16];
        buffer.Clear();
        BitConverter.TryWriteBytes(buffer, value);
        return new Guid(buffer);
    }

    private static int FromDomainGuid(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new InvalidOperationException("Owner identifier is not set.");
        }

        var bytes = id.ToByteArray();
        return BitConverter.ToInt32(bytes,0);
    }
}
