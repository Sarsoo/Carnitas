using Carnitas.Model.Operations;
using Carnitas.Model.Operations.Queued;
using MudBlazor;
using Sarsoo.Terraform.MachineReadableUI;

namespace Carnitas.Web.Extensions;

public static class DisplayExtensions
{
    extension(QueuedTaskState state)
    {
        public Color Colour => state switch
        {
            QueuedTaskState.Queued => Color.Info,
            QueuedTaskState.Processing => Color.Info,
            QueuedTaskState.Completed => Color.Success,
            QueuedTaskState.Failed => Color.Error,
            QueuedTaskState.Cancelled => Color.Info,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }

    extension(ResourceAction action)
    {
        public string Icon => action switch
        {
            ResourceAction.NoOp => Icons.Material.Filled.ArrowRight,
            ResourceAction.Create => Icons.Material.Filled.Create,
            ResourceAction.Read => Icons.Material.Filled.ArrowUpward,
            ResourceAction.Start => Icons.Material.Filled.Start,
            ResourceAction.Open => Icons.Material.Filled.FileOpen,
            ResourceAction.Close => Icons.Material.Filled.Close,
            ResourceAction.Update => Icons.Material.Filled.ArrowDownward,
            ResourceAction.Replace => Icons.Material.Filled.CompareArrows,
            ResourceAction.Delete => Icons.Material.Filled.Delete,
            ResourceAction.Move => Icons.Material.Filled.MoveDown,
            ResourceAction.Import => Icons.Material.Filled.ImportExport,
            ResourceAction.Remove => Icons.Material.Filled.RemoveCircle,
            ResourceAction.Forget => Icons.Material.Filled.RemoveCircle,
        };

        public Color Colour => action switch
        {
            ResourceAction.Create => Color.Success,
            ResourceAction.Update => Color.Warning,
            ResourceAction.Forget => Color.Warning,
            ResourceAction.Delete => Color.Error,
            ResourceAction.Replace => Color.Tertiary,
            ResourceAction.NoOp => Color.Default,
            _ => Color.Info
        };
    }
}