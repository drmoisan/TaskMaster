#nullable enable
using System;
using System.Globalization;

namespace UtilitiesCS.OutlookObjects.Store
{
    /// <summary>
    /// Pure, COM-free formatter for the single structured <c>[store-lockup]</c> WARN line emitted
    /// when a UI lockup is attributed to a store and auto-disabled (issue #264, epic #260). Mirrors
    /// <see cref="StoreFilterAttribution.FormatLine"/>: no log4net, COM, clock, or Dispatcher
    /// dependency, so it is unit-testable by a plain string assertion and is intentionally NOT marked
    /// <c>[ExcludeFromCodeCoverage]</c>. The emitted line lands in the existing WARN-filtered JSON
    /// important-logs appender with no configuration change.
    /// </summary>
    public static class StoreLockupAttribution
    {
        /// <summary>
        /// Formats exactly one structured <c>[store-lockup]</c> line capturing the store identity,
        /// the stall duration (milliseconds, "F1", <see cref="CultureInfo.InvariantCulture"/>), and
        /// the auto-disable outcome. A null/empty identity renders as <c>&lt;null&gt;</c>.
        /// </summary>
        /// <param name="identity">The attributed store identity (rendered as &lt;null&gt; when null/empty).</param>
        /// <param name="stallDuration">How long the UI thread was unresponsive.</param>
        /// <param name="autoDisabled">Whether the store was auto-disabled for this session.</param>
        /// <returns>A single-line, log-ready string.</returns>
        public static string FormatLine(string? identity, TimeSpan stallDuration, bool autoDisabled)
        {
            var name = string.IsNullOrEmpty(identity) ? "<null>" : identity;
            return string.Format(
                CultureInfo.InvariantCulture,
                "[store-lockup] identity={0} stallMs={1:F1} autoDisabled={2}",
                name,
                stallDuration.TotalMilliseconds,
                autoDisabled ? "true" : "false"
            );
        }
    }
}
