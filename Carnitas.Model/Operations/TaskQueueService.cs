using System.Text.Json;
using Carnitas.Model.Source;
using Microsoft.EntityFrameworkCore;

namespace Carnitas.Model.Operations;

public class TaskQueueService(ApplicationDbContext db, TaskQueueOptions options): ITaskQueue
{
    public async Task<QueuedTask> EnqueueAsync(EnqueueTaskRequest request, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var task = new QueuedTask
        {
            Id = Guid.NewGuid().ToString(),
            RepoUrl = request.RepoUrl,
            ModulePath = request.ModulePath,
            ModuleId = request.ModuleId,
            RepositoryId = request.RepositoryId,
            InitiatorType = request.InitiatorType,
            InitiatorUserId = request.InitiatorType == InitiatorType.User ? request.InitiatorUserId : null,
            State = QueuedTaskState.Queued,
            Priority = request.Priority,
            ScheduledAt = request.ScheduledAt ?? now,
            CreatedAt = now,
            Attempts = 0,
            MaxAttempts = request.MaxAttempts ?? options.MaxAttempts
        };

        var sequence = 0;
        foreach (var kind in request.Operations)
        {
            task.Operations.Add(new QueuedTaskOperation
            {
                Id = Guid.NewGuid().ToString(),
                QueuedTaskId = task.Id,
                Kind = kind,
                Sequence = sequence++
            });
        }

        db.QueuedTasks.Add(task);
        await db.SaveChangesAsync(ct).ConfigureAwait(false);

        return task;
    }

    public async Task<QueuedTask?> ClaimNextAsync(string workerId, TimeSpan lockDuration,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var queued = (int) QueuedTaskState.Queued;
        var processing = (int) QueuedTaskState.Processing;

        await using var transaction = await db.Database.BeginTransactionAsync(ct).ConfigureAwait(false);

        var id = await db.Database.SqlQuery<string>($"""
            SELECT "Id" AS "Value" FROM "QueuedTasks"
            WHERE ("State" = {queued} OR ("State" = {processing} AND "LockedUntil" < {now}))
              AND "ScheduledAt" <= {now}
              AND "Attempts" < "MaxAttempts"
            ORDER BY "Priority" DESC, "ScheduledAt", "CreatedAt"
            FOR UPDATE SKIP LOCKED
            LIMIT 1
            """).SingleOrDefaultAsync(ct).ConfigureAwait(false);

        if (id is null)
        {
            await transaction.RollbackAsync(ct).ConfigureAwait(false);
            return null;
        }

        var task = await db.QueuedTasks
            .Include(t => t.Operations)
            .SingleAsync(t => t.Id == id, ct)
            .ConfigureAwait(false);

        task.State = QueuedTaskState.Processing;
        task.LockedBy = workerId;
        task.LockedUntil = now + lockDuration;
        task.Attempts += 1;
        task.StartedAt ??= now;

        await db.SaveChangesAsync(ct).ConfigureAwait(false);
        await transaction.CommitAsync(ct).ConfigureAwait(false);

        return task;
    }

    public async Task<bool> RenewLeaseAsync(string taskId, string workerId, TimeSpan lockDuration,
        CancellationToken ct = default)
    {
        var lockedUntil = DateTime.UtcNow + lockDuration;

        var updated = await db.QueuedTasks
            .Where(t => t.Id == taskId
                        && t.State == QueuedTaskState.Processing
                        && t.LockedBy == workerId)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.LockedUntil, lockedUntil), ct)
            .ConfigureAwait(false);

        return updated > 0;
    }

    public async Task ReportOperationStatusAsync(string operationId, bool success, int? exitCode, string? error,
        CancellationToken ct = default)
    {
        var operation = await db.QueuedTaskOperations
            .Include(o => o.QueuedTask)
            .SingleOrDefaultAsync(o => o.Id == operationId, ct)
            .ConfigureAwait(false);

        if (operation is null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var run = await EnsureRunAsync(operation, ct).ConfigureAwait(false);
        run.EndTime = now;
        run.ExitCode = exitCode ?? (success ? 0 : 1);

        await db.SaveChangesAsync(ct).ConfigureAwait(false);

        await RecomputeTaskStateAsync(operation.QueuedTaskId, error, ct).ConfigureAwait(false);
    }

    public async Task AppendLogsAsync(IReadOnlyList<LogLine> entries, CancellationToken ct = default)
    {
        if (entries.Count == 0)
        {
            return;
        }

        foreach (var group in entries.GroupBy(e => e.OperationId))
        {
            var operation = await db.QueuedTaskOperations
                .Include(o => o.QueuedTask)
                .SingleOrDefaultAsync(o => o.Id == group.Key, ct)
                .ConfigureAwait(false);

            if (operation is null)
            {
                continue;
            }

            var run = await EnsureRunAsync(operation, ct).ConfigureAwait(false);

            var lastSequence = await db.OperationRunLogEntries
                .Where(e => e.OperationRunId == run.Id)
                .Select(e => (int?) e.Sequence)
                .MaxAsync(ct)
                .ConfigureAwait(false) ?? 0;

            foreach (var line in group)
            {
                lastSequence += 1;
                db.OperationRunLogEntries.Add(new OperationRunLogEntry
                {
                    Id = Guid.NewGuid().ToString(),
                    OperationRunId = run.Id,
                    Sequence = lastSequence,
                    Timestamp = DateTime.UtcNow,
                    Level = string.IsNullOrWhiteSpace(line.Level) ? "Information" : line.Level,
                    Type = string.IsNullOrWhiteSpace(line.Type) ? "Output" : line.Type,
                    Payload = JsonSerializer.SerializeToElement(line.Log)
                });
            }
        }

        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task SubmitPlanAsync(string operationId, string planJson, string planFilePath,
        CancellationToken ct = default)
    {
        var operation = await db.QueuedTaskOperations
            .Include(o => o.QueuedTask)
            .SingleOrDefaultAsync(o => o.Id == operationId, ct)
            .ConfigureAwait(false);

        if (operation is null)
        {
            return;
        }

        var run = await EnsureRunAsync(operation, ct).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(planFilePath))
        {
            run.LogPath = planFilePath;
        }

        await db.SaveChangesAsync(ct).ConfigureAwait(false);

        await AppendLogsAsync(new[] { new LogLine(operationId, planJson, "Information", "Plan") }, ct)
            .ConfigureAwait(false);
    }

    public async Task<int> RecordRootModulesAsync(string operationId, string? repositoryId,
        IReadOnlyList<string> relativePaths, CancellationToken ct = default)
    {
        var operation = await db.QueuedTaskOperations
            .Include(o => o.QueuedTask)
            .ThenInclude(t => t.Module)
            .SingleOrDefaultAsync(o => o.Id == operationId, ct)
            .ConfigureAwait(false);

        if (operation is null)
        {
            return 0;
        }

        var task = operation.QueuedTask;

        var resolvedRepositoryId = !string.IsNullOrWhiteSpace(repositoryId)
            ? repositoryId
            : task.RepositoryId ?? task.Module?.RepositoryId;

        if (string.IsNullOrWhiteSpace(resolvedRepositoryId))
        {
            return 0;
        }

        var repositoryExists = await db.Repository
            .AnyAsync(r => r.Id == resolvedRepositoryId, ct)
            .ConfigureAwait(false);

        if (!repositoryExists)
        {
            return 0;
        }

        var paths = relativePaths
            .Select(NormalizeRelativePath)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var existing = await db.RootModules
            .Where(m => m.RepositoryId == resolvedRepositoryId)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var existingByPath = existing
            .Where(m => !string.IsNullOrWhiteSpace(m.RelativePath))
            .GroupBy(m => m.RelativePath!, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        var desired = new HashSet<string>(paths, StringComparer.Ordinal);

        foreach (var path in paths)
        {
            var name = Path.GetFileName(path);
            if (string.IsNullOrWhiteSpace(name))
            {
                name = path;
            }

            if (existingByPath.TryGetValue(path, out var module))
            {
                module.Name = name;
                continue;
            }

            db.RootModules.Add(new RootModule
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                RelativePath = path,
                RepositoryId = resolvedRepositoryId
            });
        }

        var toRemove = existing
            .Where(m => !string.IsNullOrWhiteSpace(m.RelativePath) && !desired.Contains(m.RelativePath!))
            .ToList();

        if (toRemove.Count > 0)
        {
            var removableIds = toRemove.Select(m => m.Id).ToList();

            var referencedByRuns = await db.OperationRuns
                .Where(r => r.ModuleId != null && removableIds.Contains(r.ModuleId))
                .Select(r => r.ModuleId!)
                .Distinct()
                .ToListAsync(ct)
                .ConfigureAwait(false);

            var referencedByTasks = await db.QueuedTasks
                .Where(t => t.ModuleId != null && removableIds.Contains(t.ModuleId))
                .Select(t => t.ModuleId!)
                .Distinct()
                .ToListAsync(ct)
                .ConfigureAwait(false);

            var retained = new HashSet<string>(referencedByRuns.Concat(referencedByTasks), StringComparer.Ordinal);

            db.RootModules.RemoveRange(toRemove.Where(m => !retained.Contains(m.Id)));
        }

        await db.SaveChangesAsync(ct).ConfigureAwait(false);

        return paths.Count;
    }

    private static string NormalizeRelativePath(string path)
    {
        var normalized = path.Replace('\\', '/').Trim();
        while (normalized.StartsWith("./", StringComparison.Ordinal))
        {
            normalized = normalized[2..];
        }

        return normalized.Trim('/');
    }

    public async Task<int> SweepExpiredAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        return await db.QueuedTasks
            .Where(t => t.State == QueuedTaskState.Processing
                        && t.LockedUntil != null
                        && t.LockedUntil < now
                        && t.Attempts >= t.MaxAttempts)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.State, QueuedTaskState.Failed)
                .SetProperty(t => t.CompletedAt, now)
                .SetProperty(t => t.LastError, "lease expired after maximum attempts"), ct)
            .ConfigureAwait(false);
    }

    private async Task<OperationRun> EnsureRunAsync(QueuedTaskOperation operation, CancellationToken ct)
    {
        var run = await db.OperationRuns.FindAsync(new object?[] { operation.Id }, ct).ConfigureAwait(false);

        if (run is not null)
        {
            return run;
        }

        run = CreateRun(operation.Kind);
        run.Id = operation.Id;
        run.ModuleId = operation.QueuedTask.ModuleId;
        run.QueuedTaskId = operation.QueuedTask.Id;

        if (run is SourceDiscoveryRun discoveryRun)
        {
            discoveryRun.RepositoryId = operation.QueuedTask.RepositoryId
                ?? operation.QueuedTask.Module?.RepositoryId;
        }
        run.InitiatorType = operation.QueuedTask.InitiatorType;
        run.InitiatorUserId = operation.QueuedTask.InitiatorUserId;
        run.StartTime = operation.QueuedTask.StartedAt ?? DateTime.UtcNow;
        run.EndTime = DateTime.UtcNow;

        db.OperationRuns.Add(run);

        return run;
    }

    private async Task RecomputeTaskStateAsync(string taskId, string? error, CancellationToken ct)
    {
        var task = await db.QueuedTasks
            .Include(t => t.Operations)
            .SingleAsync(t => t.Id == taskId, ct)
            .ConfigureAwait(false);

        var operationIds = task.Operations.Select(o => o.Id).ToList();

        var runs = await db.OperationRuns
            .Where(r => r.Id != null && operationIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, ct)
            .ConfigureAwait(false);

        var anyFailure = false;
        var allSuccess = task.Operations.Count > 0;

        foreach (var operation in task.Operations)
        {
            if (!runs.TryGetValue(operation.Id, out var run) || run.ExitCode is null)
            {
                allSuccess = false;
                continue;
            }

            if (run.ExitCode != 0)
            {
                anyFailure = true;
                allSuccess = false;
            }
        }

        if (anyFailure)
        {
            task.State = QueuedTaskState.Failed;
            task.LastError = error ?? "operation failed";
            task.CompletedAt = DateTime.UtcNow;
        }
        else if (allSuccess)
        {
            task.State = QueuedTaskState.Completed;
            task.CompletedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    private static OperationRun CreateRun(OperationKind kind) => kind switch
    {
        OperationKind.Init => new InitRun(),
        OperationKind.Plan => new PlanRun(),
        OperationKind.Apply => new ApplyRun(),
        OperationKind.DiscoverSource => new SourceDiscoveryRun(),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown operation kind")
    };
}
