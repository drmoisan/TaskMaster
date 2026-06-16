using System;

namespace TaskMaster
{
    /// <summary>
    /// Records pre-measured startup phase durations and renders them as a single formatted
    /// plain-text table for diagnostic logging.
    /// </summary>
    /// <remarks>
    /// Implementations record named spans in call order and produce a deterministic table.
    /// The contract is intentionally free of any Outlook/COM, filesystem, or network
    /// dependency so it can be unit-tested without a live Outlook process.
    /// </remarks>
    internal interface IStartupTimingRecorder
    {
        /// <summary>
        /// Records a single named startup phase span. Spans are retained in call order.
        /// </summary>
        /// <param name="phaseName">
        /// The non-null name of the startup sub-component (for example "IntelConfig").
        /// </param>
        /// <param name="elapsed">The pre-measured wall-clock duration for the phase.</param>
        void RecordPhase(string phaseName, TimeSpan elapsed);

        /// <summary>
        /// Builds the formatted plain-text table of recorded phases plus a TOTAL row whose
        /// duration equals the sum of all recorded spans. Pure and deterministic given the
        /// recorded spans.
        /// </summary>
        /// <returns>The formatted table text.</returns>
        string FormatTable();

        /// <summary>
        /// Emits the formatted table via the supplied logger using <c>Info</c> level with the
        /// <c>[Startup timing]</c> prefix, consistent with the prior #139/#141 timing entries.
        /// </summary>
        /// <param name="logger">The log4net logger used to emit the table.</param>
        void EmitTable(log4net.ILog logger);
    }
}
