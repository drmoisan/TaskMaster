#nullable enable
using System;
using System.Globalization;

namespace UtilitiesCS.OutlookObjects.Store
{
    /// <summary>
    /// Pure, COM-free formatter for the <c>[store-wrapper-init]</c> diagnosis line (issue #211,
    /// Phase 3.6). Renders a single structured line summarizing one <see cref="StoreWrapper.Init"/>
    /// call: the store DisplayName, the total milliseconds spent in <c>Init</c>, and the managed
    /// thread id that ran it.
    /// </summary>
    /// <remarks>
    /// Mirrors <see cref="StoreFilterAttribution"/>: the <see cref="System.Diagnostics.Stopwatch"/>
    /// and the COM calls stay in <see cref="StoreWrapper.Init"/>; only the formatting lives here. The
    /// emit sink is a constructor-injected <see cref="Action{T}"/> so the formatting is unit-testable
    /// with a list-capturing sink and no live log4net, COM, timer, or I/O. This type is intentionally
    /// NOT marked <c>[ExcludeFromCodeCoverage]</c>.
    /// </remarks>
    public class StoreWrapperInitProbe
    {
        private readonly Action<string> _emit;

        /// <summary>
        /// Creates the probe with the line sink that will receive formatted diagnostic lines.
        /// </summary>
        /// <param name="emit">The sink invoked with each formatted line. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="emit"/> is null.</exception>
        public StoreWrapperInitProbe(Action<string> emit)
        {
            _emit = emit ?? throw new ArgumentNullException(nameof(emit));
        }

        /// <summary>
        /// Formats exactly one structured <c>[store-wrapper-init]</c> line. The <paramref name="totalMs"/>
        /// is formatted with "F1" and <see cref="CultureInfo.InvariantCulture"/>; a null
        /// <paramref name="storeDisplayName"/> renders as <c>&lt;null&gt;</c>.
        /// </summary>
        /// <param name="storeDisplayName">The store DisplayName (rendered as &lt;null&gt; when null).</param>
        /// <param name="totalMs">Total milliseconds spent in the Init call.</param>
        /// <param name="threadId">The managed thread id that ran the Init call.</param>
        /// <returns>A single-line, log-ready string.</returns>
        public string FormatLine(string? storeDisplayName, double totalMs, int threadId)
        {
            var name = storeDisplayName ?? "<null>";
            return string.Format(
                CultureInfo.InvariantCulture,
                "[store-wrapper-init] store={0} totalMs={1:F1} threadId={2}",
                name,
                totalMs,
                threadId
            );
        }

        /// <summary>
        /// Formats the line via <see cref="FormatLine"/> and routes it to the injected sink exactly once.
        /// </summary>
        /// <param name="storeDisplayName">The store DisplayName (rendered as &lt;null&gt; when null).</param>
        /// <param name="totalMs">Total milliseconds spent in the Init call.</param>
        /// <param name="threadId">The managed thread id that ran the Init call.</param>
        public void EmitLine(string? storeDisplayName, double totalMs, int threadId)
        {
            _emit(FormatLine(storeDisplayName, totalMs, threadId));
        }
    }
}
