using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;
using PetSearchHome_WEB.Infrastructure.Persistence.Entities;
using PetSearchHome_WEB.Infrastructure.Persistence;

namespace PetSearchHome_WEB.Infrastructure.Repositories;

public class EfComplaintRepository : IComplaintRepository
{
     private readonly ApplicationDbContext _db;

     public EfComplaintRepository(ApplicationDbContext db)
     {
     _db = db;
     }

     public async Task AddAsync(Complaint complaint, CancellationToken cancellationToken = default)
     {
     var entity = new ReportEntity
     {
         ReporterId = complaint.ReporterId,
         ReportedType = complaint.ReportedType,
         ReportedId = complaint.ReportedEntityId,
         Status = ReportStatus.Pending,
     CreatedAt = complaint.CreatedAt.UtcDateTime,
     Text = complaint.Reason
     };

     await _db.Reports.AddAsync(entity, cancellationToken);
     await _db.SaveChangesAsync(cancellationToken);
     }

     public async Task<IReadOnlyList<Complaint>> ListOpenAsync(CancellationToken cancellationToken = default)
     {
     var entities = await _db.Reports
     .AsNoTracking()
     .Where(r => r.Status == ReportStatus.Pending)
     .OrderByDescending(r => r.CreatedAt)
     .ToListAsync(cancellationToken);

     return entities.Select(MapToDomain).ToList();
     }

    public Task<int> CountPendingComplaintsForEntityAsync(int entityId, CancellationToken cancellationToken = default)
    {
        return _db.Reports
            .AsNoTracking()
            .CountAsync(r => r.ReportedId == entityId && r.Status == ReportStatus.Pending, cancellationToken);
    }

    public async Task UpdateStatusAsync(int id, string status, CancellationToken cancellationToken = default)
     {
     var rId = id;
     var entity = await _db.Reports.FirstOrDefaultAsync(r => r.ReportId == rId, cancellationToken);
     if (entity == null) return;

     if (string.IsNullOrWhiteSpace(status) || string.Equals(status, "pending", StringComparison.OrdinalIgnoreCase))
     {
     entity.Status = ReportStatus.Pending;
     }
     else
     {
     entity.Status = ReportStatus.Resolved;
     }

     await _db.SaveChangesAsync(cancellationToken);
     }

     private static Complaint MapToDomain(ReportEntity e)
     {
     return new Complaint
     {
     Id = e.ReportId,
     ReportedType = e.ReportedType,
         ReportedEntityId = e.ReportedId,
         ReporterId = e.ReporterId,
         Reason = e.Text ?? string.Empty,
     Status = e.Status.ToString().ToLowerInvariant(),
     CreatedAt = new DateTimeOffset(DateTime.SpecifyKind(e.CreatedAt, DateTimeKind.Utc))
     };
     }

     
}
