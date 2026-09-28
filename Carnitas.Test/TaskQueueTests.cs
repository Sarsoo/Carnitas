using Carnitas.Job;
using Carnitas.Model;
using Carnitas.Model.Governance;
using Carnitas.Model.Identity;
using Carnitas.Model.Operations;
using Carnitas.Model.Source;
using Carnitas.Model.Source.SourceControl;
using Microsoft.EntityFrameworkCore;

namespace Carnitas.Test;

public class TaskQueueTests
{
    private const string OrganisationId = "test-org";
    private const string RepositoryId = "test-repo";

    private static ApplicationDbContext CreateContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ApplicationDbContext(options);
    }

    private static bool TryGetConnectionString(out string connectionString)
    {
        connectionString = Environment.GetEnvironmentVariable("CARNITAS_TEST_DB") ?? string.Empty;
        return !string.IsNullOrWhiteSpace(connectionString);
    }

    private static async Task<string> PrepareDatabaseAsync(string connectionString)
    {
        await using var db = CreateContext(connectionString);

        await db.Database.EnsureCreatedAsync();

        await db.Database.ExecuteSqlRawAsync("""
            DELETE FROM "OperationRunLogEntries";
            DELETE FROM "ApplyRuns";
            DELETE FROM "InitRuns";
            DELETE FROM "PlanRuns";
            DELETE FROM "OperationRuns";
            DELETE FROM "QueuedTaskOperations";
            DELETE FROM "QueuedTasks";
            """);

        if (!await db.Organisations.AnyAsync(o => o.Id == OrganisationId))
        {
            db.Organisations.Add(new Organisation
            {
                Id = OrganisationId,
                Name = "Test Org",
                Description = "Test"
            });
        }

        if (!await db.Repository.AnyAsync(r => r.Id == RepositoryId))
        {
            db.Repository.Add(new Repository
            {
                Id = RepositoryId,
                Name = "Test Repo",
                Description = "Test",
                Type = RepositoryType.Generic,
                GitUrl = "git@example.com:test/repo.git",
                OrganisationId = OrganisationId
            });
        }

        await db.SaveChangesAsync();

        return OrganisationId;
    }

    private static async Task<string> CreateUserAsync(ApplicationDbContext db, string userId)
    {
        if (!await db.Users.AnyAsync(u => u.Id == userId))
        {
            db.Users.Add(new ApplicationUser
            {
                Id = userId,
                UserName = userId,
                NormalizedUserName = userId.ToUpperInvariant(),
                Email = $"{userId}@example.com",
                NormalizedEmail = $"{userId}@example.com".ToUpperInvariant(),
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            });

            await db.SaveChangesAsync();
        }

        return userId;
    }

    private static async Task<string> CreateModuleAsync(ApplicationDbContext db, string moduleId)
    {
        if (!await db.Modules.AnyAsync(m => m.Id == moduleId))
        {
            db.Modules.Add(new Module
            {
                Id = moduleId,
                Name = moduleId,
                RepositoryId = RepositoryId
            });

            await db.SaveChangesAsync();
        }

        return moduleId;
    }

    private static EnqueueTaskRequest Request(string moduleId, params OperationKind[] operations) => new(
        "git@example.com:test/repo.git",
        "terraform/module",
        moduleId,
        operations.Length == 0 ? new[] { OperationKind.Init, OperationKind.Plan } : operations);

    [Fact]
    public async Task ConcurrentClaimsAreDisjoint()
    {
        if (!TryGetConnectionString(out var connectionString))
        {
            Assert.Skip("CARNITAS_TEST_DB is not set");
            return;
        }

        await PrepareDatabaseAsync(connectionString);

        await using (var seed = CreateContext(connectionString))
        {
            var moduleId = await CreateModuleAsync(seed, "concurrent-module");
            var queue = new TaskQueueService(seed, new TaskQueueOptions());

            for (var i = 0; i < 5; i++)
            {
                await queue.EnqueueAsync(Request(moduleId));
            }
        }

        var contexts = Enumerable.Range(0, 10)
            .Select(_ => CreateContext(connectionString))
            .ToList();

        try
        {
            var options = new TaskQueueOptions();

            var results = await Task.WhenAll(contexts.Select((context, index) => Task.Run(async () =>
            {
                var queue = new TaskQueueService(context, options);
                return await queue.ClaimNextAsync($"worker-{index}", TimeSpan.FromMinutes(5));
            })));

            var claimed = results.Where(r => r is not null).Select(r => r!.Id).ToList();

            Assert.Equal(5, claimed.Count);
            Assert.Equal(5, claimed.Distinct().Count());
        }
        finally
        {
            foreach (var context in contexts)
            {
                await context.DisposeAsync();
            }
        }
    }

    [Fact]
    public async Task ExpiredLeaseIsReclaimed()
    {
        if (!TryGetConnectionString(out var connectionString))
        {
            Assert.Skip("CARNITAS_TEST_DB is not set");
            return;
        }

        await PrepareDatabaseAsync(connectionString);

        string taskId;

        await using (var seed = CreateContext(connectionString))
        {
            var moduleId = await CreateModuleAsync(seed, "reclaim-module");
            var queue = new TaskQueueService(seed, new TaskQueueOptions());
            var task = await queue.EnqueueAsync(Request(moduleId));
            taskId = task.Id;

            task.State = QueuedTaskState.Processing;
            task.LockedBy = "dead-worker";
            task.LockedUntil = DateTime.UtcNow.AddMinutes(-5);
            task.Attempts = 1;
            task.MaxAttempts = 3;
            await seed.SaveChangesAsync();
        }

        await using var db = CreateContext(connectionString);
        var reclaimed = await new TaskQueueService(db, new TaskQueueOptions())
            .ClaimNextAsync("new-worker", TimeSpan.FromMinutes(5));

        Assert.NotNull(reclaimed);
        Assert.Equal(taskId, reclaimed!.Id);
        Assert.Equal(2, reclaimed.Attempts);
        Assert.Equal("new-worker", reclaimed.LockedBy);
    }

    [Fact]
    public async Task ExhaustedLeaseIsSweptToFailed()
    {
        if (!TryGetConnectionString(out var connectionString))
        {
            Assert.Skip("CARNITAS_TEST_DB is not set");
            return;
        }

        await PrepareDatabaseAsync(connectionString);

        string taskId;

        await using (var seed = CreateContext(connectionString))
        {
            var moduleId = await CreateModuleAsync(seed, "sweep-module");
            var queue = new TaskQueueService(seed, new TaskQueueOptions());
            var task = await queue.EnqueueAsync(Request(moduleId));
            taskId = task.Id;

            task.State = QueuedTaskState.Processing;
            task.LockedBy = "dead-worker";
            task.LockedUntil = DateTime.UtcNow.AddMinutes(-5);
            task.Attempts = 3;
            task.MaxAttempts = 3;
            await seed.SaveChangesAsync();
        }

        await using var db = CreateContext(connectionString);
        var queueService = new TaskQueueService(db, new TaskQueueOptions());

        var swept = await queueService.SweepExpiredAsync();
        Assert.Equal(1, swept);

        var sweptTask = await db.QueuedTasks.SingleAsync(t => t.Id == taskId);
        Assert.Equal(QueuedTaskState.Failed, sweptTask.State);
        Assert.NotNull(sweptTask.LastError);
    }

    [Fact]
    public async Task ReportingAllOperationsCompletesTask()
    {
        if (!TryGetConnectionString(out var connectionString))
        {
            Assert.Skip("CARNITAS_TEST_DB is not set");
            return;
        }

        await PrepareDatabaseAsync(connectionString);

        string taskId;
        List<string> operationIds;

        await using (var seed = CreateContext(connectionString))
        {
            var moduleId = await CreateModuleAsync(seed, "complete-module");
            var queue = new TaskQueueService(seed, new TaskQueueOptions());
            var task = await queue.EnqueueAsync(Request(moduleId));
            taskId = task.Id;
            operationIds = task.Operations.OrderBy(o => o.Sequence).Select(o => o.Id).ToList();
        }

        await using var db = CreateContext(connectionString);
        var queueService = new TaskQueueService(db, new TaskQueueOptions());

        var claimed = await queueService.ClaimNextAsync("worker", TimeSpan.FromMinutes(5));
        Assert.NotNull(claimed);

        foreach (var operationId in operationIds)
        {
            await queueService.ReportOperationStatusAsync(operationId, true, 0, null);
        }

        var completedTask = await db.QueuedTasks.SingleAsync(t => t.Id == taskId);
        Assert.Equal(QueuedTaskState.Completed, completedTask.State);

        var runs = await db.OperationRuns.Where(r => r.QueuedTaskId == taskId).ToListAsync();
        Assert.Equal(2, runs.Count);
        Assert.All(runs, r => Assert.Equal(0, r.ExitCode));
    }

    [Fact]
    public async Task FailedOperationFailsTask()
    {
        if (!TryGetConnectionString(out var connectionString))
        {
            Assert.Skip("CARNITAS_TEST_DB is not set");
            return;
        }

        await PrepareDatabaseAsync(connectionString);

        string taskId;
        string firstOperationId;

        await using (var seed = CreateContext(connectionString))
        {
            var moduleId = await CreateModuleAsync(seed, "fail-module");
            var queue = new TaskQueueService(seed, new TaskQueueOptions());
            var task = await queue.EnqueueAsync(Request(moduleId, OperationKind.Init, OperationKind.Apply));
            taskId = task.Id;
            firstOperationId = task.Operations.OrderBy(o => o.Sequence).First().Id;
        }

        await using var db = CreateContext(connectionString);
        var queueService = new TaskQueueService(db, new TaskQueueOptions());
        await queueService.ClaimNextAsync("worker", TimeSpan.FromMinutes(5));

        await queueService.ReportOperationStatusAsync(firstOperationId, false, 1, "boom");

        var failedTask = await db.QueuedTasks.SingleAsync(t => t.Id == taskId);
        Assert.Equal(QueuedTaskState.Failed, failedTask.State);
        Assert.Equal("boom", failedTask.LastError);
    }

    [Fact]
    public async Task AppendedLogsHaveContiguousSequence()
    {
        if (!TryGetConnectionString(out var connectionString))
        {
            Assert.Skip("CARNITAS_TEST_DB is not set");
            return;
        }

        await PrepareDatabaseAsync(connectionString);

        string operationId;

        await using (var seed = CreateContext(connectionString))
        {
            var moduleId = await CreateModuleAsync(seed, "log-module");
            var queue = new TaskQueueService(seed, new TaskQueueOptions());
            var task = await queue.EnqueueAsync(Request(moduleId));
            operationId = task.Operations.OrderBy(o => o.Sequence).First().Id;
        }

        await using var db = CreateContext(connectionString);
        var queueService = new TaskQueueService(db, new TaskQueueOptions());
        await queueService.ClaimNextAsync("worker", TimeSpan.FromMinutes(5));

        await queueService.AppendLogsAsync(new[]
        {
            new LogLine(operationId, "{\"message\":\"one\"}", "Information", LogType.Json),
            new LogLine(operationId, "{\"message\":\"two\"}", "Information", LogType.Json),
            new LogLine(operationId, "{\"message\":\"three\"}", "Information", LogType.Json)
        });

        var entries = await db.OperationRunLogEntries
            .Where(e => e.OperationRunId == operationId)
            .OrderBy(e => e.Sequence)
            .ToListAsync();

        Assert.Equal(3, entries.Count);
        Assert.Equal(new[] { 1, 2, 3 }, entries.Select(e => e.Sequence).ToArray());
    }

    [Fact]
    public async Task DefaultEnqueueIsAttributedToSystem()
    {
        if (!TryGetConnectionString(out var connectionString))
        {
            Assert.Skip("CARNITAS_TEST_DB is not set");
            return;
        }

        await PrepareDatabaseAsync(connectionString);

        await using var db = CreateContext(connectionString);
        var moduleId = await CreateModuleAsync(db, "system-attribution-module");
        var queue = new TaskQueueService(db, new TaskQueueOptions());

        var task = await queue.EnqueueAsync(Request(moduleId));

        Assert.Equal(InitiatorType.System, task.InitiatorType);
        Assert.Null(task.InitiatorUserId);
    }

    [Fact]
    public async Task UserInitiatorPropagatesToOperationRuns()
    {
        if (!TryGetConnectionString(out var connectionString))
        {
            Assert.Skip("CARNITAS_TEST_DB is not set");
            return;
        }

        await PrepareDatabaseAsync(connectionString);

        const string userId = "attribution-user";
        string taskId;
        List<string> operationIds;

        await using (var seed = CreateContext(connectionString))
        {
            var moduleId = await CreateModuleAsync(seed, "attribution-module");
            await CreateUserAsync(seed, userId);

            var queue = new TaskQueueService(seed, new TaskQueueOptions());
            var task = await queue.EnqueueAsync(Request(moduleId) with
            {
                InitiatorType = InitiatorType.User,
                InitiatorUserId = userId
            });

            taskId = task.Id;
            operationIds = task.Operations.OrderBy(o => o.Sequence).Select(o => o.Id).ToList();

            Assert.Equal(InitiatorType.User, task.InitiatorType);
            Assert.Equal(userId, task.InitiatorUserId);
        }

        await using var db = CreateContext(connectionString);
        var queueService = new TaskQueueService(db, new TaskQueueOptions());
        await queueService.ClaimNextAsync("worker", TimeSpan.FromMinutes(5));

        foreach (var operationId in operationIds)
        {
            await queueService.ReportOperationStatusAsync(operationId, true, 0, null);
        }

        var runs = await db.OperationRuns.Where(r => r.QueuedTaskId == taskId).ToListAsync();

        Assert.Equal(operationIds.Count, runs.Count);
        Assert.All(runs, r => Assert.Equal(InitiatorType.User, r.InitiatorType));
        Assert.All(runs, r => Assert.Equal(userId, r.InitiatorUserId));
    }

    [Fact]
    public async Task EnqueueCreatesOperationRuns()
    {
        if (!TryGetConnectionString(out var connectionString))
        {
            Assert.Skip("CARNITAS_TEST_DB is not set");
            return;
        }

        await PrepareDatabaseAsync(connectionString);

        await using var db = CreateContext(connectionString);
        var moduleId = await CreateModuleAsync(db, "enqueue-runs-module");
        var queue = new TaskQueueService(db, new TaskQueueOptions());

        var task = await queue.EnqueueAsync(Request(moduleId, OperationKind.Init, OperationKind.Plan) with
        {
            GitReference = "feature/x",
            RepositoryId = RepositoryId
        });

        Assert.Equal(2, task.Operations.Count);

        var runs = await db.OperationRuns
            .Where(r => r.QueuedTaskId == task.Id)
            .ToListAsync();

        Assert.Equal(2, runs.Count);
        Assert.Contains(runs, r => r is InitRun);
        Assert.Contains(runs, r => r is PlanRun);

        foreach (var operation in task.Operations)
        {
            var run = runs.Single(r => r.Id == operation.Id);
            Assert.Equal(task.Id, run.QueuedTaskId);
            Assert.Equal(moduleId, run.ModuleId);
            Assert.Equal("feature/x", run.GitReference);
            Assert.Equal(InitiatorType.System, run.InitiatorType);
            Assert.Null(run.InitiatorUserId);
            Assert.Null(run.StartTime);
            Assert.Null(run.EndTime);
        }
    }

    [Fact]
    public async Task ConcurrentLogBatchesDoNotDuplicateRuns()
    {
        if (!TryGetConnectionString(out var connectionString))
        {
            Assert.Skip("CARNITAS_TEST_DB is not set");
            return;
        }

        await PrepareDatabaseAsync(connectionString);

        string operationId;

        await using (var seed = CreateContext(connectionString))
        {
            var moduleId = await CreateModuleAsync(seed, "concurrent-logs-module");
            var queue = new TaskQueueService(seed, new TaskQueueOptions());
            var task = await queue.EnqueueAsync(Request(moduleId));
            operationId = task.Operations.OrderBy(o => o.Sequence).First().Id;
        }

        var contexts = Enumerable.Range(0, 2)
            .Select(_ => CreateContext(connectionString))
            .ToList();

        try
        {
            await Task.WhenAll(contexts.Select((context, index) => Task.Run(async () =>
            {
                var queue = new TaskQueueService(context, new TaskQueueOptions());
                await queue.AppendLogsAsync(new[]
                {
                    new LogLine(operationId, $"{{\"message\":\"batch-{index}\"}}", "Information", LogType.Json)
                });
            })));
        }
        finally
        {
            foreach (var context in contexts)
            {
                await context.DisposeAsync();
            }
        }

        await using var db = CreateContext(connectionString);

        var runCount = await db.OperationRuns.CountAsync(r => r.Id == operationId);
        Assert.Equal(1, runCount);

        var logCount = await db.OperationRunLogEntries.CountAsync(e => e.OperationRunId == operationId);
        Assert.Equal(2, logCount);
    }

    [Fact]
    public async Task ClaimStampsRunStartTime()
    {
        if (!TryGetConnectionString(out var connectionString))
        {
            Assert.Skip("CARNITAS_TEST_DB is not set");
            return;
        }

        await PrepareDatabaseAsync(connectionString);

        await using var db = CreateContext(connectionString);
        var moduleId = await CreateModuleAsync(db, "claim-start-module");
        var queue = new TaskQueueService(db, new TaskQueueOptions());
        var task = await queue.EnqueueAsync(Request(moduleId));
        var operationId = task.Operations.OrderBy(o => o.Sequence).First().Id;

        var claimed = await queue.ClaimNextAsync("worker", TimeSpan.FromMinutes(5));
        Assert.NotNull(claimed);

        var firstRun = await db.OperationRuns.SingleAsync(r => r.Id == operationId);
        Assert.NotNull(firstRun.StartTime);
        var firstStart = firstRun.StartTime;

        claimed!.LockedUntil = DateTime.UtcNow.AddMinutes(-5);
        await db.SaveChangesAsync();

        var reclaimed = await queue.ClaimNextAsync("worker-2", TimeSpan.FromMinutes(5));
        Assert.NotNull(reclaimed);

        var secondRun = await db.OperationRuns.SingleAsync(r => r.Id == operationId);
        Assert.Equal(firstStart, secondRun.StartTime);
    }

    [Fact]
    public async Task SourceDiscoveryRunGetsRepositoryIdAtEnqueue()
    {
        if (!TryGetConnectionString(out var connectionString))
        {
            Assert.Skip("CARNITAS_TEST_DB is not set");
            return;
        }

        await PrepareDatabaseAsync(connectionString);

        await using var db = CreateContext(connectionString);
        var moduleId = await CreateModuleAsync(db, "discovery-repo-module");
        var queue = new TaskQueueService(db, new TaskQueueOptions());

        var explicitRepo = await queue.EnqueueAsync(
            Request(moduleId, OperationKind.DiscoverSource) with { RepositoryId = RepositoryId });

        var explicitRun = Assert.IsType<SourceDiscoveryRun>(await db.OperationRuns
            .SingleAsync(r => r.QueuedTaskId == explicitRepo.Id));
        Assert.Equal(RepositoryId, explicitRun.RepositoryId);

        var inferredRepo = await queue.EnqueueAsync(Request(moduleId, OperationKind.DiscoverSource));

        var inferredRun = Assert.IsType<SourceDiscoveryRun>(await db.OperationRuns
            .SingleAsync(r => r.QueuedTaskId == inferredRepo.Id));
        Assert.Equal(RepositoryId, inferredRun.RepositoryId);
    }

    [Fact]
    public async Task MissingRunIsSkipped()
    {
        if (!TryGetConnectionString(out var connectionString))
        {
            Assert.Skip("CARNITAS_TEST_DB is not set");
            return;
        }

        await PrepareDatabaseAsync(connectionString);

        await using var db = CreateContext(connectionString);
        var moduleId = await CreateModuleAsync(db, "missing-run-module");
        var queue = new TaskQueueService(db, new TaskQueueOptions());
        var task = await queue.EnqueueAsync(Request(moduleId));
        var operationId = task.Operations.OrderBy(o => o.Sequence).First().Id;

        var run = await db.OperationRuns.SingleAsync(r => r.Id == operationId);
        db.OperationRuns.Remove(run);
        await db.SaveChangesAsync();

        await queue.AppendLogsAsync(new[]
        {
            new LogLine(operationId, "{\"message\":\"orphan\"}", "Information", LogType.Json)
        });
        await queue.SubmitPlanAsync(operationId, "{\"plan\":true}", "/tmp/plan", CancellationToken.None);
        await queue.ReportOperationStatusAsync(operationId, true, 0, null);

        var runCount = await db.OperationRuns.CountAsync(r => r.Id == operationId);
        Assert.Equal(0, runCount);

        var logCount = await db.OperationRunLogEntries.CountAsync(e => e.OperationRunId == operationId);
        Assert.Equal(0, logCount);
    }
}
