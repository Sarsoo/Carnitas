using System.Diagnostics;
using OpenTelemetry;

namespace Carnitas.Observability;

public static class BaggageTagMapper
{
    private static bool _isSet;
    private static readonly Lock _isSetLock = new();

    public static void MapBaggageToTags()
    {
        lock (_isSetLock)
        {
            if (!_isSet)
            {
                ActivitySource.AddActivityListener(new ActivityListener
                {
                    ShouldListenTo = _ => true,
                    ActivityStopped = activity =>
                    {
                        foreach (var v in Baggage.GetBaggage())
                        {
                            activity.AddTag(v.Key, v.Value);
                        }
                    }
                });

                _isSet = true;
            }
        }
    }
}