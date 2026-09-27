using System.Diagnostics;

namespace Carnitas.Web.Observability;

public class Tracing
{
    public static readonly ActivitySource Source = new("Carnitas.Web.Observability");
}