using Carnitas.Model.Operations;
using Sarsoo.Terraform.Command;

namespace Carnitas.Model.Extensions;

public static class OperationExtensions
{
    extension(OperationRun run)
    {
        public string DisplayString => run switch
        {
            ApplyRun => "Apply",
            InitRun => "Init",
            PlanRun => "Plan",
            SourceDiscoveryRun sourceDiscoveryRun => "Source Discovery",
            _ => throw new ArgumentOutOfRangeException(nameof(run))
        };

        public string ToDisplayString() => run.DisplayString;
    }
    
    extension(OperationKind kind)
    {
        public string DisplayString => kind switch
        {
            OperationKind.Apply => "Apply",
            OperationKind.Init => "Init",
            OperationKind.Plan => "Plan",
            OperationKind.DiscoverSource => "Source Discovery",
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

        public string ToDisplayString() => kind.DisplayString;
    }
}