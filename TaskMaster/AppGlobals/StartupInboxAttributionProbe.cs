using System;
using System.Globalization;

namespace TaskMaster
{
    /// <summary>
    /// Diagnosis-only, behavior-preserving attribution formatter for issue #211 (PostLoad /
    /// LoadInboxes probe). Holds the pure line-formatting logic for two startup probes added to
    /// pinpoint the ~121 s PostLoad STA freeze: the per-step <c>[readiness-hookup]</c> markers in
    /// <c>AppEvents.PerformReadinessHookup</c> and the per-store <c>[loadinboxes]</c> attribution in
    /// <c>AppOlObjects.LoadInboxes</c>. Every line is emitted through an injected sink so the
    /// formatting is unit-testable without a live appender, live COM, a live timer, or live GC reads.
    /// </summary>
    /// <remarks>
    /// This type is intentionally NOT marked <c>[ExcludeFromCodeCoverage]</c>: it contains the
    /// coverable formatting seam (AC19), mirroring <see cref="StartupDiagnosticsProbe"/>. The
    /// COM/STA-bound concerns stay in the thin call sites in <c>AppEvents</c> and <c>AppOlObjects</c>:
    /// the per-operation <see cref="System.Diagnostics.Stopwatch"/>, the live
    /// <c>Globals.Ol.*</c> COM reads, and the <c>store.GetDefaultFolder</c> / <c>ShouldIncludeStore</c>
    /// calls. This helper only formats numeric and string values supplied by those call sites. No
    /// <c>Stopwatch</c>, no <c>GC</c>, and no banned timing APIs (<c>DateTime.Now</c>,
    /// <c>DateTime.UtcNow</c>, <c>Random.Shared</c>, <c>Thread.Sleep</c>, <c>Task.Delay</c>) are used
    /// here.
    /// </remarks>
    public sealed class StartupInboxAttributionProbe
    {
        private readonly Action<string> _emit;

        /// <summary>
        /// Creates a probe that emits structured attribution lines through the supplied sink.
        /// </summary>
        /// <param name="emit">
        /// The line sink. Production passes <c>s =&gt; logger.Debug(s)</c>; tests pass a delegate
        /// that captures lines into a list. Must not be null.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="emit"/> is null.</exception>
        public StartupInboxAttributionProbe(Action<string> emit)
        {
            _emit = emit ?? throw new ArgumentNullException(nameof(emit));
        }

        /// <summary>
        /// Formats the START marker for one readiness-hookup step, e.g.
        /// <c>[readiness-hookup] step=Inboxes start</c>. The last START with no matching END before
        /// the freeze names the blocking operation.
        /// </summary>
        /// <param name="step">The step name (one of <c>ToDoFolder.Items</c>, <c>OlReminders</c>, <c>Inboxes</c>). Emitted verbatim.</param>
        /// <returns>The exact <c>[readiness-hookup] step=&lt;step&gt; start</c> line.</returns>
        public static string FormatReadinessHookupStart(string step)
        {
            return $"[readiness-hookup] step={step} start";
        }

        /// <summary>
        /// Formats the END marker for one readiness-hookup step, e.g.
        /// <c>[readiness-hookup] step=Inboxes end elapsedMs=12.34</c>.
        /// </summary>
        /// <param name="step">The step name. Emitted verbatim.</param>
        /// <param name="elapsedMs">The measured elapsed milliseconds for the step (from a call-site <see cref="System.Diagnostics.Stopwatch"/>).</param>
        /// <returns>The exact <c>[readiness-hookup] step=&lt;step&gt; end elapsedMs=&lt;F2&gt;</c> line, F2 invariant-culture.</returns>
        public static string FormatReadinessHookupEnd(string step, double elapsedMs)
        {
            return $"[readiness-hookup] step={step} end elapsedMs={elapsedMs.ToString("F2", CultureInfo.InvariantCulture)}";
        }

        /// <summary>
        /// Formats one per-store attribution line for <c>AppOlObjects.LoadInboxes</c>, e.g.
        /// <c>[loadinboxes] store=Mailbox shouldIncludeMs=1.23 included=true getDefaultFolderMs=4.56</c>.
        /// The <c>getDefaultFolderMs</c> field is rendered with its value only when the store was
        /// included (so the <c>GetDefaultFolder</c> COM call ran); when the store was excluded it is
        /// rendered as <c>getDefaultFolderMs=n/a</c> because <c>GetDefaultFolder</c> is not invoked.
        /// The line whose <c>shouldIncludeMs</c> or <c>getDefaultFolderMs</c> is multi-second names the
        /// blocking store and whether the block is in <c>ShouldIncludeStore</c> (FilePath read) or
        /// <c>GetDefaultFolder</c>.
        /// </summary>
        /// <param name="displayName">
        /// The store's <c>DisplayName</c>, emitted verbatim. The caller is responsible for the guarded
        /// read (returning a sentinel such as <c>&lt;unavailable&gt;</c> if the read throws).
        /// </param>
        /// <param name="shouldIncludeMs">The measured elapsed ms of the <c>ShouldIncludeStore</c> call.</param>
        /// <param name="included">Whether the store was included by <c>ShouldIncludeStore</c>.</param>
        /// <param name="getDefaultFolderMs">
        /// The measured elapsed ms of <c>GetDefaultFolder(olFolderInbox)</c> when <paramref name="included"/>
        /// is true; ignored (rendered as <c>n/a</c>) when <paramref name="included"/> is false.
        /// </param>
        /// <returns>The exact single attribution line, with F2 invariant-culture numeric formatting.</returns>
        public static string FormatLoadInboxesStore(
            string displayName,
            double shouldIncludeMs,
            bool included,
            double? getDefaultFolderMs
        )
        {
            var includedText = included ? "true" : "false";
            var getDefaultFolderText =
                included && getDefaultFolderMs.HasValue
                    ? getDefaultFolderMs.Value.ToString("F2", CultureInfo.InvariantCulture)
                    : "n/a";
            return $"[loadinboxes] store={displayName} "
                + $"shouldIncludeMs={shouldIncludeMs.ToString("F2", CultureInfo.InvariantCulture)} "
                + $"included={includedText} "
                + $"getDefaultFolderMs={getDefaultFolderText}";
        }

        /// <summary>
        /// Emits exactly one <c>[readiness-hookup] step=&lt;step&gt; start</c> line through the sink.
        /// </summary>
        /// <param name="step">The step name. Emitted verbatim.</param>
        public void EmitReadinessHookupStart(string step)
        {
            _emit(FormatReadinessHookupStart(step));
        }

        /// <summary>
        /// Emits exactly one <c>[readiness-hookup] step=&lt;step&gt; end elapsedMs=&lt;F2&gt;</c> line
        /// through the sink.
        /// </summary>
        /// <param name="step">The step name. Emitted verbatim.</param>
        /// <param name="elapsedMs">The measured elapsed milliseconds for the step.</param>
        public void EmitReadinessHookupEnd(string step, double elapsedMs)
        {
            _emit(FormatReadinessHookupEnd(step, elapsedMs));
        }

        /// <summary>
        /// Emits exactly one <c>[loadinboxes]</c> per-store attribution line through the sink.
        /// </summary>
        /// <param name="displayName">The store's guarded <c>DisplayName</c>. Emitted verbatim.</param>
        /// <param name="shouldIncludeMs">The measured elapsed ms of the <c>ShouldIncludeStore</c> call.</param>
        /// <param name="included">Whether the store was included.</param>
        /// <param name="getDefaultFolderMs">
        /// The measured elapsed ms of <c>GetDefaultFolder(olFolderInbox)</c> when included; ignored when excluded.
        /// </param>
        public void EmitLoadInboxesStore(
            string displayName,
            double shouldIncludeMs,
            bool included,
            double? getDefaultFolderMs
        )
        {
            _emit(
                FormatLoadInboxesStore(displayName, shouldIncludeMs, included, getDefaultFolderMs)
            );
        }
    }
}
