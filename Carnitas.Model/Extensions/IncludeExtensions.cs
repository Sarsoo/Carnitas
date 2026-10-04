using Carnitas.Model.Operations.Run;
using Microsoft.EntityFrameworkCore;

namespace Carnitas.Model.Extensions;

public static class IncludeExtensions
{
    extension(IQueryable<OperationRunLogEntry> query)
    {
        public IQueryable<OperationRunLogEntry> IncludeOrgHierarchy() => query
            .Include(r => r.OperationRun)
            .ThenInclude(r => r.QueuedTask)
            .Include(r => r.OperationRun)
            .ThenInclude(r => r.Module)
            .ThenInclude(r => r.Repository)
            .ThenInclude(r => r.Organisation);
    }
}