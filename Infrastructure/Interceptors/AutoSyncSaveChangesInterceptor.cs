using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PerformansSitesi.Domain.Entities;

namespace PerformansSitesi.Infrastructure.Interceptors;

public class AutoSyncSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ILogger<AutoSyncSaveChangesInterceptor> _logger;

    public AutoSyncSaveChangesInterceptor(ILogger<AutoSyncSaveChangesInterceptor> logger)
    {
        _logger = logger;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        LogChanges(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        LogChanges(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void LogChanges(DbContext? context)
    {
        if (context == null) return;

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added ||
                       e.State == EntityState.Modified ||
                       e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            var entityName = entry.Entity.GetType().Name;
            var state = entry.State.ToString();

            switch (entry.State)
            {
                case EntityState.Added:
                    _logger.LogInformation($"[DB SYNC] YENİ KAYIT: {entityName}");
                    LogEntityDetails(entry, "EKLENEN");
                    break;

                case EntityState.Modified:
                    _logger.LogInformation($"[DB SYNC] GÜNCELLEME: {entityName}");
                    LogModifiedProperties(entry);
                    break;

                case EntityState.Deleted:
                    _logger.LogInformation($"[DB SYNC] SİLME: {entityName}");
                    LogEntityDetails(entry, "SİLİNEN");
                    break;
            }
        }
    }

    private void LogEntityDetails(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, string action)
    {
        var properties = entry.Properties
            .Where(p => p.Metadata.Name != "SifreHash") 
            .Select(p => $"{p.Metadata.Name}: {p.CurrentValue}")
            .ToList();

        _logger.LogDebug($"[{action}] {string.Join(", ", properties)}");
    }

    private void LogModifiedProperties(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        var modifiedProps = entry.Properties
            .Where(p => p.IsModified && p.Metadata.Name != "SifreHash")
            .Select(p => $"{p.Metadata.Name}: {p.OriginalValue} ? {p.CurrentValue}")
            .ToList();

        if (modifiedProps.Any())
        {
            _logger.LogDebug($"[DEĞER ALANLAR] {string.Join(", ", modifiedProps)}");
        }
    }
}
