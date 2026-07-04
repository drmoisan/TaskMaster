using System;
using System.Threading;
using UtilitiesCS;

namespace QuickFiler
{
    public partial class EfcHomeController
    {
        private static string DescribeSynchronizationContext(SynchronizationContext syncContext)
        {
            return syncContext?.GetType().FullName ?? "null";
        }

        private static string DescribeStartupOverlapState(IApplicationGlobals globals)
        {
            return globals?.Events is null ? "unknown" : "correlated";
        }

        private static string BuildFirstSelectionTimingContext(
            IApplicationGlobals globals,
            int selectedItemCount
        )
        {
            return $"threadId={Thread.CurrentThread.ManagedThreadId}; syncContext={DescribeSynchronizationContext(SynchronizationContext.Current)}; selectedItemCount={selectedItemCount}; startupOverlapState={DescribeStartupOverlapState(globals)}";
        }

        private static void LogFirstSelectionTiming(
            string phase,
            IApplicationGlobals globals,
            int selectedItemCount,
            string details = null
        )
        {
            var detailSegment = string.IsNullOrWhiteSpace(details) ? string.Empty : $" | {details}";
            var phaseLabel = phase.StartsWith("[First-selection timing]", StringComparison.Ordinal)
                ? phase
                : $"[First-selection timing] {phase}";
            logger.Debug(
                $"{phaseLabel} | {BuildFirstSelectionTimingContext(globals, selectedItemCount)}{detailSegment}"
            );
        }
    }
}
