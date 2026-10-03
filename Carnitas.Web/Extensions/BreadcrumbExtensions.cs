using Carnitas.Model.Operations;
using Carnitas.Model.Source;
using Carnitas.Model.Source.SourceControl;
using Carnitas.Web.Components.Shared;
using MudBlazor;

namespace Carnitas.Web.Extensions;

public static class BreadcrumbExtensions
{
    extension(Repository repo)
    {
        public IReadOnlyList<BreadcrumbItem> ToBreadrumbs()
        {
            var list = new List<BreadcrumbItem>(2)
            {
                new(
                    repo.Organisation?.Name ?? string.Empty, 
                    repo.Organisation?.ToLink(),
                    icon: Constants.OrgIcon
                ),
                new(
                    repo.Name,
                    href: repo.ToLink(),
                    icon: Constants.RepoIcon
                ),
            };

            return list;
        }
    }
    
    extension(Module module)
    {
        public IReadOnlyList<BreadcrumbItem> ToBreadrumbs()
        {
            var list = new List<BreadcrumbItem>(3)
            {
                new(
                    module.Repository?.Organisation?.Name ?? string.Empty, 
                    module.Repository?.Organisation?.ToLink(),
                    icon: Constants.OrgIcon
                ),
                new(
                    module.Repository?.Name ?? string.Empty, 
                    module.Repository?.ToLink(),
                    icon: Constants.RepoIcon
                ),
                new(
                    module.Name, 
                    href: module.ToLink(),
                    icon: Constants.ModuleIcon
                ),
            };

            return list;
        }
    }
    
    extension(QueuedTask task)
    {
        public IReadOnlyList<BreadcrumbItem> ToBreadrumbs()
        {
            var list = new List<BreadcrumbItem>(4)
            {
                new(
                    task.Module?.Repository?.Organisation?.Name ?? string.Empty, 
                    task.Module?.Repository?.Organisation?.ToLink(),
                    icon: Constants.OrgIcon
                ),
                new(
                    task.Module?.Repository?.Name ?? string.Empty, 
                    task.Module?.Repository?.ToLink(),
                    icon: Constants.RepoIcon
                ),
                new(
                    task.Module?.Name ?? string.Empty, 
                    task.Module?.ToLink(),
                    icon: Constants.ModuleIcon
                ),
                new(
                    "Task", 
                    href: task.ToLink(),
                    icon: Constants.OpIcon
                ),
            };

            return list;
        }
    }
    
    extension(OperationRun run)
    {
        public IReadOnlyList<BreadcrumbItem> ToBreadrumbs()
        {
            var list = new List<BreadcrumbItem>(3)
            {
                new(
                    run.Module?.Repository?.Organisation?.Name ?? string.Empty, 
                    run.Module?.Repository?.Organisation?.ToLink(),
                    icon: Constants.OrgIcon
                ),
                new(
                    run.Module?.Repository?.Name ?? string.Empty, 
                    run.Module?.Repository?.ToLink(),
                    icon: Constants.RepoIcon
                ),
                new(
                    run.Module?.Name ?? string.Empty, 
                    run.Module?.ToLink(),
                    icon: Constants.ModuleIcon
                ),
                new("Task", run.QueuedTask?.ToLink(),
                    icon: Constants.OpIcon),
                new(run.GetType().Name, run.ToLink()),
            };

            return list;
        }
    }
}