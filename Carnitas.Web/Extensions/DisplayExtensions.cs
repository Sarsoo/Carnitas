using Carnitas.Model.Operations;
using Carnitas.Model.Operations.Queued;
using MudBlazor;

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
}