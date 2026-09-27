using System.Diagnostics;

namespace Carnitas.Observability;

public static class UnhandledExceptionHandler
{
    private static bool _isSet;
    private static readonly Lock _isSetLock = new();

    private static void UnhandledExcaptionHandler(object source, UnhandledExceptionEventArgs args)
    {
        try
        {
            UnobservedTaskExceptionHandler((Exception)args.ExceptionObject);
        }
        catch (Exception)
        {
        }
    }

    private static void UnobservedTaskHandler(object? source, UnobservedTaskExceptionEventArgs args)
    {
        try
        {
            UnobservedTaskExceptionHandler(args.Exception);
        }
        catch (Exception)
        {
        }
    }

    private static void UnobservedTaskExceptionHandler(Exception ex)
    {
        var activity = Activity.Current;

        while (activity != null)
        {
            activity.AddException(ex);
            activity.Dispose();
            activity = activity.Parent;
        }
    }

    public static void SetUnhandledExceptionHandler()
    {
        lock (_isSetLock)
        {
            if (!_isSet)
            {
                AppDomain.CurrentDomain.UnhandledException += UnhandledExcaptionHandler;
                TaskScheduler.UnobservedTaskException += UnobservedTaskHandler;
                _isSet = true;
            }
        }
    }
}