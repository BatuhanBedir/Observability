using System.Diagnostics;

namespace Observability.ConsoleApp;

internal static class ActivitySourceProvider //tracedatagenerate
{
    internal static ActivitySource Source = new(OpenTelemetryConstants.ActivitySourceName);

    internal static ActivitySource SourceFile = new(OpenTelemetryConstants.ActivitySourceName);
}
