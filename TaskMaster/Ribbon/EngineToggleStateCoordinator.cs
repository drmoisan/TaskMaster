using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UtilitiesCS;

namespace TaskMaster
{
    /// <summary>
    /// The issue #505/#506/#518 state coordinator for the Spam and Triage engine-activation
    /// toggle checkboxes: a synchronous last-known-state cache answering Office's
    /// <c>getPressed</c> poll, a lazy asynchronous prime that corrects that cache, and the awaited
    /// toggle path whose ordering guarantees Office never re-queries stale state.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Office's <c>checkBox</c> <c>getPressed</c> contract is a <b>synchronous</b>
    /// <c>bool</c>-returning callback polled on the Outlook STA, but the truth
    /// (<see cref="IAppItemEngines.EngineActiveAsync"/>) sits behind an awaited configuration
    /// load. Blocking the STA to bridge that gap is prohibited: ribbon controller paths install a
    /// <c>WindowsFormsSynchronizationContext</c> on that thread, so a continuation posted back to
    /// a blocked STA is a deterministic deadlock, and the first configuration await triggers a
    /// full classifier-configuration disk load that would freeze menu-open. This type resolves the
    /// mismatch with a cache instead: the read is a dictionary lookup, and correctness is restored
    /// asynchronously by invalidating the control once the real value is known.
    /// </para>
    /// <para>
    /// Engine <em>readiness</em> is deliberately not consulted. <see cref="EngineReadinessGate"/>
    /// probes <c>InboxEngines</c>, from which an engine configured off is filtered out, so a
    /// readiness-gated toggle could never re-enable a disabled engine. Toggle state is backed by
    /// configuration, which is why these four call sites do not route through
    /// <c>RunEngineCommandAsync</c>.
    /// </para>
    /// <para>
    /// This type is deliberately NOT marked <c>[ExcludeFromCodeCoverage]</c>: it is host-neutral
    /// decision logic with no COM, no <c>Microsoft.Office.*</c> reference, no <c>MessageBox</c>,
    /// no WinForms type, and no logger reference — logging is an injected delegate. It follows the
    /// <see cref="EngineGatedCommandRunner"/> precedent and is fully unit-tested. The only
    /// STA-affine operation, <c>IRibbonUI.InvalidateControl</c>, stays behind the injected
    /// <c>invalidateControl</c> delegate whose production implementation marshals through
    /// <c>UtilitiesCS.UiThread.Dispatcher</c>.
    /// </para>
    /// </remarks>
    internal sealed partial class EngineToggleStateCoordinator
    {
        private readonly Func<IAppItemEngines> _enginesAccessor;
        private readonly Action<string> _invalidateControl;
        private readonly Action<string> _notifyUnavailable;
        private readonly Action<string, Exception> _logError;

        /// <summary>
        /// Serializes the at-most-one-prime decision. Held only across a dictionary probe, the
        /// marker registration, and the start of the prime; no await occurs inside it.
        /// </summary>
        private readonly object _primeGate = new object();

        /// <summary>
        /// Last-known activation state per engine key, each stamped with the ticket of the read
        /// that produced it. A key absent from this cache has never been primed successfully and
        /// reports as unchecked. The compare-and-apply semantics live in the cache type.
        /// </summary>
        private readonly EngineTogglePressedStateCache _pressedState =
            new EngineTogglePressedStateCache();

        /// <summary>
        /// The registration marker per engine key: registered before the prime starts, removed by
        /// <see cref="CompletePrime"/> when the prime faults or is canceled, and retained after a
        /// successful prime. Its presence is the at-most-one-prime guard; its value is the
        /// test-observable handle returned by <see cref="GetPrimeTask"/>.
        /// </summary>
        private readonly ConcurrentDictionary<string, Task> _primeTasks = new ConcurrentDictionary<
            string,
            Task
        >(StringComparer.Ordinal);

        /// <summary>
        /// Prime failures already reported (issue #948), keyed by engine and base-exception type;
        /// a repeat of a reported kind is not logged again. Never cleared: a cached key never primes.
        /// </summary>
        private readonly ConcurrentDictionary<
            (string EngineName, Type FaultType),
            byte
        > _reportedPrimeFaults = new ConcurrentDictionary<(string, Type), byte>();

        /// <summary>
        /// Creates a coordinator over an engines accessor and three injected sinks.
        /// </summary>
        /// <param name="enginesAccessor">
        /// Supplies the current engines container. Must not be null, and must not throw: its
        /// result is read outside any guard by <see cref="GetPressed"/> and by the refusal check
        /// of <see cref="HandleToggleClickAsync"/>, so an exception it raised would escape both.
        /// It is expected to return null before the ribbon controller has been given its globals,
        /// which this type treats as "state unknown" rather than as an error.
        /// </param>
        /// <param name="invalidateControl">
        /// Receives a ribbon control id whenever the cached state behind that control changes, so
        /// Office re-queries <c>getPressed</c>. Must not be null.
        /// </param>
        /// <param name="notifyUnavailable">
        /// Receives exactly one message per toggle click refused because the engines are not
        /// available. Presentation is the sink's concern. Must not be null. The call is guarded
        /// (issue #964): an exception it throws is reported once through
        /// <paramref name="logError"/> and is not rethrown.
        /// </param>
        /// <param name="logError">
        /// Receives an observed prime fault, toggle fault or notification failure as a message
        /// plus the exception. Must not be null. The call is guarded: an exception it throws is
        /// discarded, because no further reporting channel remains.
        /// </param>
        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        internal EngineToggleStateCoordinator(
            Func<IAppItemEngines> enginesAccessor,
            Action<string> invalidateControl,
            Action<string> notifyUnavailable,
            Action<string, Exception> logError
        )
        {
            _enginesAccessor =
                enginesAccessor ?? throw new ArgumentNullException(nameof(enginesAccessor));
            _invalidateControl =
                invalidateControl ?? throw new ArgumentNullException(nameof(invalidateControl));
            _notifyUnavailable =
                notifyUnavailable ?? throw new ArgumentNullException(nameof(notifyUnavailable));
            _logError = logError ?? throw new ArgumentNullException(nameof(logError));
        }

        /// <summary>
        /// The synchronous <c>getPressed</c> answer for an engine toggle, plus a lazy prime when
        /// the state is not yet known.
        /// </summary>
        /// <param name="engineName">The engine key; ordinal, case-sensitive.</param>
        /// <returns>
        /// The cached activation state, or <see langword="false"/> when the key is null,
        /// whitespace, unmapped, or has never been primed. This method performs a dictionary read
        /// only: it never awaits, never blocks, and never throws while the engines accessor
        /// honours its non-throwing precondition.
        /// </returns>
        /// <remarks>
        /// On a cache miss with the engines available, at most one prime per key is started; a
        /// second read while a prime is in flight starts no second prime. When the prime succeeds
        /// it stores the value and invalidates the mapped control, so Office re-queries and the
        /// checkbox corrects itself. With the engines unavailable nothing is started, which is the
        /// correct pre-<c>SetGlobals</c> degradation.
        /// </remarks>
        internal bool GetPressed(string engineName)
        {
            if (!EngineToggleCatalog.TryGetControlId(engineName, out var controlId))
            {
                return false;
            }

            if (_pressedState.TryGetActive(engineName, out var cached))
            {
                return cached;
            }

            StartPrimeIfNeeded(engineName, controlId);
            return false;
        }

        /// <summary>
        /// The toggle-click boundary: the only <c>catch</c> clause in this type that observes an
        /// engine fault. Every sink call on this path goes through <see cref="TryInvokeSink"/>,
        /// which holds the only other <c>catch</c> clause.
        /// </summary>
        /// <param name="engineName">The engine key whose activation setting is being flipped.</param>
        /// <returns>
        /// A task that completes when the toggle path has completed or its fault has been
        /// observed.
        /// </returns>
        /// <remarks>
        /// When the engines are not available the click is refused with exactly one
        /// <c>notifyUnavailable</c> message and no engine member is invoked. That notification is
        /// guarded (issue #964): if the sink throws, its exception is reported once through
        /// <c>logError</c>. Otherwise <see cref="ExecuteToggleAsync"/> runs inside a single
        /// boundary <c>try</c>/<c>catch</c>: a fault is reported through <c>logError</c>, is not
        /// rethrown, and does not invalidate. Every <c>logError</c> call is itself guarded
        /// (issue #947): it is the last reporting channel, so a failure inside it has nowhere
        /// else to go and is discarded deliberately, following
        /// <c>RibbonCommandBoundary.SafeLog</c>. This method therefore never throws on either
        /// path, even when both sinks throw, provided the engines accessor honours its
        /// non-throwing precondition, because its caller is an <c>async void</c> Office handler
        /// whose faults would otherwise become unobserved.
        /// </remarks>
        internal async Task HandleToggleClickAsync(string engineName)
        {
            if (_enginesAccessor() is null)
            {
                if (
                    !TryInvokeSink(
                        () => _notifyUnavailable(BuildUnavailableMessage(engineName)),
                        out var notifyFailure
                    )
                )
                {
                    _ = TryInvokeSink(
                        () => _logError(BuildNotifyFailedMessage(engineName), notifyFailure),
                        out _
                    );
                }

                return;
            }

            try
            {
                await ExecuteToggleAsync(engineName).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _ = TryInvokeSink(() => _logError(BuildToggleFailedMessage(engineName), ex), out _);
            }
        }

        /// <summary>
        /// The testable core of the toggle path: flip the setting, re-read the truth, update the
        /// cache, then invalidate the control — in exactly that order.
        /// </summary>
        /// <param name="engineName">The engine key; must be a mapped toggle key.</param>
        /// <returns>A task that completes once the control has been invalidated.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="engineName"/> is null, whitespace, or not a mapped toggle key.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The engines are not available. Callers reach this method through
        /// <see cref="HandleToggleClickAsync"/>, which refuses that case first; the guard exists so
        /// a direct caller fails explicitly rather than with a null dereference.
        /// </exception>
        /// <remarks>
        /// This method contains no <c>catch</c> of any kind, so it can never degenerate into a
        /// swallow-all: an engine fault propagates unchanged to the boundary. Updating the cache
        /// <b>before</b> invalidating is the load-bearing invariant — Office answers an
        /// invalidation by re-querying <c>getPressed</c>, so invalidating first would be answered
        /// from stale state.
        /// </remarks>
        internal async Task ExecuteToggleAsync(string engineName)
        {
            if (!EngineToggleCatalog.TryGetControlId(engineName, out var controlId))
            {
                throw new ArgumentException(
                    BuildUnmappedKeyMessage(engineName),
                    nameof(engineName)
                );
            }

            var engines = _enginesAccessor();
            if (engines is null)
            {
                throw new InvalidOperationException(BuildUnavailableMessage(engineName));
            }

            await engines.ToggleEngineAsync(engineName).ConfigureAwait(false);

            // The ticket is taken after the toggle completes and before the activation read,
            // because that is the moment this observation window opens.
            var sequence = _pressedState.NextSequence();
            var active = await engines.EngineActiveAsync(engineName).ConfigureAwait(false);

            if (_pressedState.TryApplyState(engineName, active, sequence))
            {
                _invalidateControl(controlId);
            }
        }

        /// <summary>
        /// Invokes one injected sink and contains any exception it throws (issues #947 and #964).
        /// This holds the only <c>catch</c> clause in this type that intercepts a sink failure;
        /// every <c>notifyUnavailable</c> and <c>logError</c> call goes through it.
        /// </summary>
        /// <param name="sinkCall">The sink invocation, with its arguments already bound.</param>
        /// <param name="sinkFailure">
        /// The exception the sink threw, or <see langword="null"/> when the sink returned
        /// normally.
        /// </param>
        /// <returns>
        /// <see langword="true"/> when the sink returned normally; <see langword="false"/> when
        /// it threw. The exception is never rethrown.
        /// </returns>
        /// <remarks>
        /// The caller decides what happens to a contained failure, following
        /// <c>RibbonCommandBoundary.ReportFailure</c>: the refusal path of
        /// <see cref="HandleToggleClickAsync"/> forwards a notification failure to the log sink,
        /// and every log-sink caller discards a log failure because no further channel remains.
        /// <see cref="CompletePrime"/> records a reported fault kind only when this method
        /// returns <see langword="true"/>, so a sink that throws leaves the report owed
        /// (issue #948).
        /// </remarks>
        private static bool TryInvokeSink(Action sinkCall, out Exception sinkFailure)
        {
            try
            {
                sinkCall();
                sinkFailure = null;
                return true;
            }
            catch (Exception ex)
            {
                sinkFailure = ex;
                return false;
            }
        }
    }
}
