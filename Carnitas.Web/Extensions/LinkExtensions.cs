using Carnitas.Model.Governance;
using Carnitas.Model.Operations;
using Carnitas.Model.Operations.Queued;
using Carnitas.Model.Operations.Run;
using Carnitas.Model.Source;
using Carnitas.Model.Source.SourceControl;
using MudBlazor;

namespace Carnitas.Web.Extensions;

public static class LinkExtensions
{
    extension(DataGridRowClickEventArgs<Repository> repo)
    {
        public string ToLink() => repo.Item.ToLink();
    }
    
    extension(Repository repo)
    {
        public string ToLink() => $"/Repo/{repo.Id}";
    }
    
    extension(DataGridRowClickEventArgs<Organisation> org)
    {
        public string ToLink() => org.Item.ToLink();
    }
    
    extension(Organisation org)
    {
        public string ToLink() => $"/Org/{org.Id}";
    }
    
    extension(DataGridRowClickEventArgs<Module> module)
    {
        public string ToLink() => module.Item.ToLink();
    }
    
    extension(Module module)
    {
        public string ToLink() => $"/Repo/{module.RepositoryId}/Module/{module.Id}";
    }
    
    extension(DataGridRowClickEventArgs<OperationRun> run)
    {
        public string ToLink() => run.Item.ToLink();
    }
    
    extension(OperationRun run)
    {
        public string ToLink() => $"/Task/Run/{run.Id}";
    }
    
    extension(DataGridRowClickEventArgs<QueuedTaskOperation> run)
    {
        public string ToLink() => run.Item.ToLink();
    }
    
    extension(QueuedTaskOperation run)
    {
        public string ToLink() => $"/Task/Run/{run.Id}";
    }
    
    extension(DataGridRowClickEventArgs<QueuedTask> task)
    {
        public string ToLink() => task.Item.ToLink();
    }
    
    extension(QueuedTask task)
    {
        public string ToLink() => $"/Task/{task.Id}";
    }
}