using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
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
    internal sealed class EngineToggleStateCoordinator
    {
        /// <summary>
        /// Rendered in place of an engine key when the caller supplied null or empty, so a message
        /// is never ambiguous about which key was seen.
        /// </summary>
        private const string NullEngineNameToken = "(null)";

        private readonly Func<IAppItemEngines> _enginesAccessor;
        private readonly Action<string> _invalidateControl;
        private readonly Action<string> _notifyUnavailable;
        private readonly Action<string, Exception> _logError;

        /// <summary>
        /// Serializes the at-most-one-prime decision. Held only across a dictionary probe and a
        /// task start; no await occurs inside it.
        /// </summary>
        private readonly object _primeGate = new object();

        /// <summary>
        /// Last-known activation state per engine key. A key absent from this map has never been
        /// primed successfully and reports as unchecked.
        /// </summary>
        private readonly ConcurrentDictionary<string, bool> _pressedState =
            new ConcurrentDictionary<string, bool>(StringComparer.Ordinal);

        /// <summary>
        /// The in-flight — or most recently completed — prime per engine key. Its presence is the
        /// at-most-one-prime guard; its value is the test-observable handle returned by
        /// <see cref="GetPrimeTask"/>.
        /// </summary>
        private readonly ConcurrentDictionary<string, Task> _primeTasks =
            new ConcurrentDictionary<string, Task>(StringComparer.Ordinal);

        /// <summary>
        /// Creates a coordinator over an engines accessor and three injected sinks.
        /// </summary>
        /// <param name="enginesAccessor">
        /// Supplies the current engines container. Must not be null, but is expected to return
        /// null before the ribbon controller has been given its globals, which this type treats as
        /// "state unknown" rather than as an error.
        /// </param>
        /// <param name="invalidateControl">
        /// Receives a ribbon control id whenever the cached state behind that control changes, so
        /// Office re-queries <c>getPressed</c>. Must not be null.
        /// </param>
        /// <param name="notifyUnavailable">
        /// Receives exactly one message per toggle click refused because the engines are not
        /// available. Presentation is the sink's concern. Must not be null.
        /// </param>
        /// <param name="logError">
        /// Receives an observed prime or toggle fault as a message plus the exception. Must not be
        /// null.
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
        /// only: it never awaits, never blocks, and never throws.
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

            if (_pressedState.TryGetValue(engineName, out var cached))
            {
                return cached;
            }

            StartPrimeIfNeeded(engineName, controlId);
            return false;
        }

        /// <summary>
        /// The toggle-click boundary: the only place in this type that observes a fault with a
        /// <c>catch</c> clause.
        /// </summary>
        /// <param name="engineName">The engine key whose activation setting is being flipped.</param>
        /// <returns>
        /// A task that completes when the toggle path has completed or its fault has been
        /// observed.
        /// </returns>
        /// <remarks>
        /// When the engines are not available the click is refused with exactly one
        /// <c>notifyUnavailable</c> message and nothing else is invoked. Otherwise
        /// <see cref="ExecuteToggleAsync"/> runs inside a single boundary <c>try</c>/<c>catch</c>:
        /// a fault is reported through <c>logError</c>, is not rethrown, and does not invalidate.
        /// This method never throws, because its caller is an <c>async void</c> Office handler
        /// whose faults would otherwise become unobserved.
        /// </remarks>
        internal async Task HandleToggleClickAsync(string engineName)
        {
            if (_enginesAccessor() is null)
            {
                _notifyUnavailable(BuildUnavailableMessage(engineName));
                return;
            }

            try
            {
                await ExecuteToggleAsync(engineName).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logError(BuildToggleFailedMessage(engineName), ex);
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
            var active = await engines.EngineActiveAsync(engineName).ConfigureAwait(false);

            _pressedState[engineName] = active;
            _invalidateControl(controlId);
        }

        /// <summary>
        /// The in-flight — or most recently completed — prime for an engine key, exposed so tests
        /// can await the prime deterministically instead of polling or sleeping.
        /// </summary>
        /// <param name="engineName">The engine key; ordinal, case-sensitive.</param>
        /// <returns>
        /// The prime task, or <see cref="Task.CompletedTask"/> when no prime has been started for
        /// the key. The returned task never faults: a prime fault is observed inside the prime
        /// itself and reported through <c>logError</c>.
        /// </returns>
        internal Task GetPrimeTask(string engineName)
        {
            if (string.IsNullOrEmpty(engineName))
            {
                return Task.CompletedTask;
            }

            return _primeTasks.TryGetValue(engineName, out var prime) ? prime : Task.CompletedTask;
        }

        /// <summary>
        /// Starts the single prime for an engine key, unless one is already registered or the
        /// engines are not yet available.
        /// </summary>
        private void StartPrimeIfNeeded(string engineName, string controlId)
        {
            var engines = _enginesAccessor();
            if (engines is null)
            {
                return;
            }

            lock (_primeGate)
            {
                if (_primeTasks.ContainsKey(engineName))
                {
                    return;
                }

                _primeTasks[engineName] = StartObservedPrime(engines, engineName, controlId);
            }
        }

        /// <summary>
        /// Runs <see cref="ApplyPrimeAsync"/> and attaches the fault observer.
        /// </summary>
        /// <remarks>
        /// The observer is a continuation rather than a <c>catch</c> clause, so this type keeps
        /// exactly one <c>catch</c> — the click boundary. Reading
        /// <see cref="Task.Exception"/> inside <see cref="CompletePrime"/> marks the fault
        /// observed, so no unobserved task remains. The returned continuation task always
        /// completes successfully, which is what makes it safe for a test to await.
        /// </remarks>
        private Task StartObservedPrime(
            IAppItemEngines engines,
            string engineName,
            string controlId
        )
        {
            return ApplyPrimeAsync(engines, engineName, controlId)
                .ContinueWith(
                    completed => CompletePrime(completed, engineName),
                    CancellationToken.None,
                    TaskContinuationOptions.None,
                    TaskScheduler.Default
                );
        }

        /// <summary>
        /// Reads the real activation state once, stores it, and invalidates the mapped control.
        /// Contains no <c>catch</c>: a fault propagates into the returned task, where
        /// <see cref="CompletePrime"/> observes it.
        /// </summary>
        private async Task ApplyPrimeAsync(
            IAppItemEngines engines,
            string engineName,
            string controlId
        )
        {
            var active = await engines.EngineActiveAsync(engineName).ConfigureAwait(false);
            _pressedState[engineName] = active;
            _invalidateControl(controlId);
        }

        /// <summary>
        /// Observes the outcome of a prime. On failure the cache is left unset — so the key still
        /// reports unchecked — the in-flight marker is cleared so a later read may re-prime, and
        /// the fault is reported through <c>logError</c>.
        /// </summary>
        private void CompletePrime(Task completed, string engineName)
        {
            var failure = completed.Exception;
            if (failure is null)
            {
                return;
            }

            _primeTasks.TryRemove(engineName, out _);
            _logError(BuildPrimeFailedMessage(engineName), failure.GetBaseException());
        }

        /// <summary>
        /// Renders an engine key for inclusion in a message, so a null key is never ambiguous.
        /// </summary>
        private static string RenderEngineName(string engineName)
        {
            return string.IsNullOrEmpty(engineName) ? NullEngineNameToken : engineName;
        }

        /// <summary>
        /// The message emitted when a toggle click is refused because the engines are unavailable.
        /// </summary>
        private static string BuildUnavailableMessage(string engineName)
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                "The engine '{0}' is not available yet, so its enable/disable setting cannot be "
                    + "changed. Please try again once initialization completes.",
                RenderEngineName(engineName)
            );
        }

        /// <summary>
        /// The message logged when the toggle path faults.
        /// </summary>
        private static string BuildToggleFailedMessage(string engineName)
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                "Toggling the enable/disable setting for engine '{0}' failed.",
                RenderEngineName(engineName)
            );
        }

        /// <summary>
        /// The message logged when the state prime faults.
        /// </summary>
        private static string BuildPrimeFailedMessage(string engineName)
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                "Reading the activation state for engine '{0}' failed; its toggle continues to "
                    + "report unchecked.",
                RenderEngineName(engineName)
            );
        }

        /// <summary>
        /// The message carried by the <see cref="ArgumentException"/> for an unmapped engine key.
        /// </summary>
        private static string BuildUnmappedKeyMessage(string engineName)
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                "The engine key '{0}' has no toggle checkbox in EngineToggleCatalog.",
                RenderEngineName(engineName)
            );
        }
    }
}
