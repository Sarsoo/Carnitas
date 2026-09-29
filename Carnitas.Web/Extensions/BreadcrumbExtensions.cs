using Carnitas.Model.Operations;
using Carnitas.Model.Source;
using Carnitas.Model.Source.SourceControl;
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
                    icon: Icons.Material.Filled.Domain
                ),
                new(
                    repo.Name,
                    href: repo.ToLink(),
                    icon: Icons.Material.Filled.Code
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
                    icon: Icons.Material.Filled.Domain
                ),
                new(
                    module.Repository?.Name ?? string.Empty, 
                    module.Repository?.ToLink(),
                    icon: Icons.Material.Filled.Code
                ),
                new(
                    module.Name, 
                    href: module.ToLink(),
                    icon: Icons.Material.Filled.SnippetFolder
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
                    icon: Icons.Material.Filled.Domain
                ),
                new(
                    task.Module?.Repository?.Name ?? string.Empty, 
                    task.Module?.Repository?.ToLink(),
                    icon: Icons.Material.Filled.Code
                ),
                new(
                    task.Module?.Name ?? string.Empty, 
                    task.Module?.ToLink(),
                    icon: Icons.Material.Filled.SnippetFolder
                ),
                new(
                    task.GetType().Name, 
                    href: task.ToLink(),
                    disabled: true
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
                    icon: Icons.Material.Filled.Domain
                ),
                new(
                    run.Module?.Repository?.Name ?? string.Empty, 
                    run.Module?.Repository?.ToLink(),
                    icon: Icons.Material.Filled.Code
                ),
                new(
                    run.Module?.Name ?? string.Empty, 
                    run.Module?.ToLink(),
                    icon: Icons.Material.Filled.SnippetFolder
                ),
                new(run.GetType().Name, run.ToLink()),
            };

            return list;
        }
    }
}