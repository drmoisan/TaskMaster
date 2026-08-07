using System;

namespace QuickFiler.Controllers
{
    /// <summary>
    /// Maps the confidence gate's <c>(scanned, accepted, quantity)</c> progress signal onto the
    /// 0-30 progress band that <c>QfcHomeController.RunAsync</c> owns, and forwards the mapped value
    /// with a human-readable scanning label.
    /// </summary>
    /// <remarks>
    /// Issue #424. Extracted from <c>QfcHomeController</c> so the mapping decision logic is unit
    /// testable: <c>QfcDatamodel</c> is <c>[ExcludeFromCodeCoverage]</c> and the controller is
    /// COM/UI-bound, whereas this type is pure. It performs no I/O, touches no UI, creates no
    /// threads, and holds no Outlook references — callers route the reported value through
    /// <c>ProgressTracker</c>, which marshals to the UI thread.
    ///
    /// Contract: <see cref="Report"/> is expected to be called once per scanned candidate with
    /// monotonically non-decreasing inputs. The reported value is clamped to <c>[0, 30]</c> and is
    /// itself monotonically non-decreasing even if a caller supplies a regressing input, so the
    /// progress bar can never travel backwards.
    /// </remarks>
    internal sealed class QfcScanProgressBandMapper
    {
        /// <summary>Ceiling of the progress band this mapper reports into.</summary>
        internal const int BandCeiling = 30;

        private readonly Action<double, string> _report;
        private double _lastValue;

        /// <param name="report">
        /// Sink for the mapped <c>(value, label)</c> pair. Typed <c>double</c> so that
        /// <c>ProgressTracker.Report(double value, string jobName)</c> binds directly by method-group
        /// conversion.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="report"/> is <see langword="null"/>.
        /// </exception>
        internal QfcScanProgressBandMapper(Action<double, string> report)
        {
            _report = report ?? throw new ArgumentNullException(nameof(report));
        }

        /// <summary>
        /// Maps one gate progress signal into the band and forwards it to the sink.
        /// </summary>
        /// <param name="scanned">Candidates scored so far. Used for the label only.</param>
        /// <param name="accepted">Candidates accepted so far. Drives the mapped value.</param>
        /// <param name="quantity">
        /// Target batch size. A non-positive value yields 0, because no meaningful fraction exists.
        /// </param>
        internal void Report(int scanned, int accepted, int quantity)
        {
            double value =
                quantity <= 0 ? 0 : Math.Round((double)BandCeiling * accepted / quantity);

            if (value > BandCeiling)
            {
                value = BandCeiling;
            }

            if (value < 0)
            {
                value = 0;
            }

            // Never travel backwards, even if a caller supplies a regressing input.
            if (value < _lastValue)
            {
                value = _lastValue;
            }

            _lastValue = value;
            _report(
                value,
                $"Scanning for high-confidence items ({scanned} scanned, {accepted} accepted)"
            );
        }
    }
}
