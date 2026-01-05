using Core.Model.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Core.Data.Helper;

public class TimestampInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
    {
        IEnumerable<EntityEntry<IHasTimestamps>> entries =
            eventData.Context?.ChangeTracker.Entries<IHasTimestamps>() ?? [];
        DateTime now = DateTime.UtcNow;

        foreach (EntityEntry<IHasTimestamps> entry in entries)
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        return base.SavingChangesAsync(eventData, result, ct);
    }
}