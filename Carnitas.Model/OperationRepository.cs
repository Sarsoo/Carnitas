using Carnitas.Model.Operations.Queued;

namespace Carnitas.Model;

public class OperationRepository(ApplicationDbContext db)
{
    public IQueryable<QueuedTask> GetModuleQueuedTasks(string moduleId)
    {
        return db.QueuedTasks.Where(q => q.ModuleId == moduleId);
    }
}