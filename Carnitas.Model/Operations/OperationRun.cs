using Carnitas.Model.Source;

namespace Carnitas.Model.Operations;

public class OperationRun
{
    public string Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public int? ExitCode { get; set; }
    public string? LogPath { get; set; }

    public string? GitReference { get; set; }
    public string? CommitSha { get; set; }

    public string RootModuleId { get; set; }
    public RootModule RootModule { get; set; }

    public string? CheckoutId { get; set; }
    public Checkout? Checkout { get; set; }
}
