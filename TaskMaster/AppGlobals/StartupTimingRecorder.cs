using System;
using System.Collections.Generic;
using System.Linq;
using UtilitiesCS;

namespace TaskMaster
{
    /// <summary>
    /// Production <see cref="IStartupTimingRecorder"/> that accumulates pre-measured startup
    /// phase spans in its own ordered collection and renders them as a single formatted table.
    /// </summary>
    /// <remarks>
    /// This recorder deliberately does NOT wrap <c>UtilitiesCS.HelperClasses.SegmentStopWatch</c>:
    /// <c>SegmentStopWatch.GetDurations()</c> derives its TOTAL row from the watch's own
    /// <c>Elapsed</c>, which is <see cref="TimeSpan.Zero"/> when fed pre-measured (injected)
    /// spans, so wrapping it would yield an always-zero TOTAL. Instead, the TOTAL row here is the
    /// sum of all recorded spans. Column alignment is reused (not reimplemented) via
    /// <c>UtilitiesCS.PrettyPrinters.ToFormattedText(string[][], ...)</c> — the same overload
    /// <c>SegmentStopWatch.GetDurations()</c> calls.
    /// </remarks>
    internal sealed class StartupTimingRecorder : IStartupTimingRecorder
    {
        // Duration format mirrors the existing SegmentStopWatch.GetDurations() convention.
        private const string DurationFormat = "%m\\:ss\\.ff";

        private readonly List<(string PhaseName, TimeSpan Elapsed)> _phases = new();

        /// <summary>
        /// The recorded phase names in call order. Exposed for test observability of the
        /// recording sequence; consumed only within the assembly and its test assembly.
        /// </summary>
        internal IReadOnlyList<string> RecordedPhaseNames =>
            _phases.Select(p => p.PhaseName).ToList();

        /// <inheritdoc />
        public void RecordPhase(string phaseName, TimeSpan elapsed)
        {
            if (phaseName is null)
            {
                throw new ArgumentNullException(nameof(phaseName));
            }

            _phases.Add((phaseName, elapsed));
        }

        /// <inheritdoc />
        public string FormatTable()
        {
            var total = TimeSpan.FromTicks(_phases.Sum(p => p.Elapsed.Ticks));

            var rows = _phases
                .Select(p => new[] { p.Elapsed.ToString(DurationFormat), p.PhaseName })
                .Append(new[] { total.ToString(DurationFormat), "TOTAL" })
                .ToArray();

            return rows.ToFormattedText(
                ["Duration", "Action"],
                [Enums.Justification.Right, Enums.Justification.Left]
            );
        }

        /// <inheritdoc />
        public void EmitTable(log4net.ILog logger)
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            logger.Info($"[Startup timing]\n{FormatTable()}");
        }
    }

    /// <summary>
    /// No-op <see cref="IStartupTimingRecorder"/> used on the flag-off path so the startup
    /// coordinator can invoke the recorder unconditionally without recording or emitting anything.
    /// </summary>
    internal sealed class NullStartupTimingRecorder : IStartupTimingRecorder
    {
        /// <inheritdoc />
        public void RecordPhase(string phaseName, TimeSpan elapsed)
        {
            // Intentionally no-op: timing is disabled.
        }

        /// <inheritdoc />
        public string FormatTable() => string.Empty;

        /// <inheritdoc />
        public void EmitTable(log4net.ILog logger)
        {
            // Intentionally no-op: timing is disabled.
        }
    }
}
