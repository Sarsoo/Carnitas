using Carnitas.Model.Identity;
using Carnitas.Model.Source;
using Carnitas.Model.Source.SourceControl;

namespace Carnitas.Model.Operations;

public class QueuedTask
{
    public string Id { get; set; }

    public string RepoUrl { get; set; }
    public string ModulePath { get; set; }

    public string? GitReference { get; set; }

    public string? ModuleId { get; set; }
    public Module? Module { get; set; }

    public string? RepositoryId { get; set; }
    public Repository? Repository { get; set; }

    public InitiatorType InitiatorType { get; set; }
    public string? InitiatorUserId { get; set; }
    public ApplicationUser? InitiatorUser { get; set; }

    public QueuedTaskState State { get; set; }

    public int Priority { get; set; }
    public DateTime ScheduledAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public int Attempts { get; set; }
    public int MaxAttempts { get; set; }

    public string? LockedBy { get; set; }
    public DateTime? LockedUntil { get; set; }

    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? LastError { get; set; }

    public ICollection<QueuedTaskOperation> Operations { get; set; } = new List<QueuedTaskOperation>();
}
