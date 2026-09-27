using System.Diagnostics;

namespace Carnitas.Observability;

public static class Tracing
{
    public static readonly ActivitySource Source = new ActivitySource("Carnitas.Observability");
}