using Carnitas.Model.Governance;
using Carnitas.Model.Operations;
using Carnitas.Model.Source.SourceControl;

namespace Carnitas.Model.Source;

public class RootModule: Module
{ 
    public string? TrackingBranch { get; set; }
}